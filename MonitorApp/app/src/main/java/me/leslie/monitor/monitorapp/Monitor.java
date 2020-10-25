package me.leslie.monitor.monitorapp;

import android.annotation.SuppressLint;
import android.content.Context;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.FrameLayout;
import android.widget.TextView;

import androidx.appcompat.app.ActionBar;
import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;

import org.json.JSONObject;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import pl.droidsonroids.gif.GifDrawable;
import pl.droidsonroids.gif.GifImageView;

/**
 * An example full-screen activity that shows and hides the system UI (i.e.
 * status bar and navigation/system bar) with user interaction.
 */
public class Monitor extends AppCompatActivity {
    /**
     * Whether or not the system UI should be auto-hidden after
     * {@link #AUTO_HIDE_DELAY_MILLIS} milliseconds.
     */
    private static final boolean AUTO_HIDE = true;

    /**
     * If {@link #AUTO_HIDE} is set, the number of milliseconds to wait after
     * user interaction before hiding the system UI.
     */
    private static final int AUTO_HIDE_DELAY_MILLIS = 3000;

    /**
     * Some older devices needs a small delay between UI widget updates
     * and a change of the status and navigation bar.
     */
    private static final int UI_ANIMATION_DELAY = 300;
    private final Handler mHideHandler = new Handler();
    private View mContentView;
    private final Runnable mHidePart2Runnable = new Runnable() {
        @SuppressLint("InlinedApi")
        @Override
        public void run() {
            // Delayed removal of status and navigation bar

            // Note that some of these constants are new as of API 16 (Jelly Bean)
            // and API 19 (KitKat). It is safe to use them, as they are inlined
            // at compile-time and do nothing on earlier devices.
            mContentView.setSystemUiVisibility(View.SYSTEM_UI_FLAG_LOW_PROFILE
                    | View.SYSTEM_UI_FLAG_FULLSCREEN
                    | View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                    | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
                    | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                    | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION);

        }
    };
    private View mControlsView;
    private final Runnable mShowPart2Runnable = new Runnable() {
        @Override
        public void run() {
            // Delayed display of UI elements
            ActionBar actionBar = getSupportActionBar();
            if (actionBar != null) {
                actionBar.show();
            }
            mControlsView.setVisibility(View.VISIBLE);
        }
    };
    private boolean mVisible;
    private final Runnable mHideRunnable = new Runnable() {
        @Override
        public void run() {
            hide();
        }
    };
    /**
     * Touch listener to use for in-layout UI controls to delay hiding the
     * system UI. This is to prevent the jarring behavior of controls going away
     * while interacting with activity UI.
     */
    private final View.OnTouchListener mDelayHideTouchListener = new View.OnTouchListener() {
        @Override
        public boolean onTouch(View view, MotionEvent motionEvent) {
            if (AUTO_HIDE) {
                delayedHide(AUTO_HIDE_DELAY_MILLIS);
            }
            return false;
        }
    };


    private TextView cpuTemp;
    private TextView gpuTemp;
    private GifImageView gifImageView;
    private FrameLayout background;

    private Context context = this;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_monitor);

        mVisible = true;
        mContentView = findViewById(R.id.fullscreen_content);

        cpuTemp = findViewById(R.id.CPU_TEMP);
        gpuTemp = findViewById(R.id.GPU_TEMP);
        gifImageView = findViewById(R.id.GIF);
        background = findViewById(R.id.fullscreen_content);

        cpuTemp.setText("CPU: NO DATA");
        gpuTemp.setText("GPU: NO DATA");

    }

    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        super.onPostCreate(savedInstanceState);

        // Trigger the initial hide() shortly after the activity has been
        // created, to briefly hint to the user that UI controls
        // are available.
        Thread temperatureServer = new TemperatureServer();
        Thread TCPServer = new TCPServer(8080);
        temperatureServer.start();
        TCPServer.start();


        delayedHide(100);
    }


    private void hide() {
        // Hide UI first
        ActionBar actionBar = getSupportActionBar();
        if (actionBar != null) {
            actionBar.hide();
        }
        mVisible = false;

        // Schedule a runnable to remove the status and navigation bar after a delay
        mHideHandler.removeCallbacks(mShowPart2Runnable);
        mHideHandler.postDelayed(mHidePart2Runnable, UI_ANIMATION_DELAY);
    }

    /**
     * Schedules a call to hide() in delay milliseconds, canceling any
     * previously scheduled calls.
     */
    private void delayedHide(int delayMillis) {
        mHideHandler.removeCallbacks(mHideRunnable);
        mHideHandler.postDelayed(mHideRunnable, delayMillis);
    }

    private class TemperatureServer extends Thread {
        private boolean keepRunning = true;
        private float lastCpuData;

        private float lastGpuData;

        public void run() {
            byte[] buffer = new byte[8];
            DatagramPacket packet = new DatagramPacket(buffer, buffer.length);

            try {
                DatagramSocket socket = new DatagramSocket(8080);

                while(keepRunning) {
                    socket.receive(packet);
                    byte[] data = packet.getData();
                    lastCpuData = ByteBuffer.wrap(Arrays.copyOfRange(data, 0, 4)).order(ByteOrder.LITTLE_ENDIAN).getFloat();
                    lastGpuData = ByteBuffer.wrap(Arrays.copyOfRange(data, 4, 8)).order(ByteOrder.LITTLE_ENDIAN).getFloat();
                    //sleep(1000);
                    // lastCpuData = new Random().nextFloat();
                    // lastGpuData = new Random().nextFloat();
                    Log.d("Monitoring", "CPU: " + lastCpuData);
                    Log.d("Monitoring", "GPU: " + lastGpuData);
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            cpuTemp.setText("CPU: " + lastCpuData + "°C");
                            gpuTemp.setText("GPU: " + lastGpuData + "°C");
                        }
                    });
                }
                socket.close();
            } catch (Throwable e) {
                e.printStackTrace();
            }

        }

        public void kill() {
            keepRunning = false;
        }

        public float getLastCpuData() {
            return lastCpuData;
        }

        public float getLastGpuData() {
            return lastGpuData;
        }
    }

    private class TCPServer extends Thread {
        private ServerSocket server;
        private final int port;

        public TCPServer(int port) {
            this.port = port;
        }

        public void run() {
            try {
                server = new ServerSocket(port);
                while (server != null){
                    Thread worker = new TCPWorker(server.accept());
                    worker.start();
                }
            } catch (IOException e){
                Log.e("Monitor-TCP", e.getMessage());
            }
        }

        public void kill() {
            try {
                server.close();
            } catch (IOException e){
                Log.e("Monitor-TCPServer", e.getMessage());
            }
            server = null;
        }
    }



    private class TCPWorker extends Thread {
        private final Socket WORKER_CONNECTION;
        public TCPWorker(Socket workerConnection){
            this.WORKER_CONNECTION = workerConnection;
        }

        public void run(){
            try {
                ReceivedData receivedData = readReceivedData();
                parseData(receivedData);
            } catch (IOException e){
                Log.e("Monitor-TCPWorker", e.getMessage());
            }
        }

        private ReceivedData readReceivedData() throws IOException{
            ReceivedData receivedData = new ReceivedData(WORKER_CONNECTION.getInputStream());
            WORKER_CONNECTION.close();
            return receivedData;
        }

        private void parseData(ReceivedData input){
            try {
                byte type = input.getPrefix();
                switch (type) {
                    case 0:
                        layoutConfig(input);
                        break;
                    case 1:
                        staticBackgroundImage(input);
                        break;
                    case 2:
                        gifBackgroundImage(input);
                        break;
                    default:
                        Log.e("Monitor-TCPWorker", "Invalid Package received: " + type);
                }
            } catch (Exception e){
                Log.e("Monitor-TCPWorker", e.getMessage());
            }
        }

        private void layoutConfig(ReceivedData data){
            try {
                String json = convertByteArrayToString(data.getData());
                JSONObject parsedJson = new JSONObject(json);
                final int gpuYFromBottom = parsedJson.getInt("gpu_y_from_bottom");
                final int cpuYFromTop = parsedJson.getInt("cpu_y_from_top");
                String readColour = parsedJson.getString("text_colour");
                final int textColour = parseInt(readColour);
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        try {
                            ConstraintLayout.LayoutParams gpuConstraints = (ConstraintLayout.LayoutParams) gpuTemp.getLayoutParams();
                            gpuConstraints.bottomMargin = gpuYFromBottom;
                            Log.d("Monitor-LayoutConfig", "gpu: " + gpuConstraints.bottomToBottom);
                            gpuTemp.requestLayout();
                            gpuTemp.setTextColor(textColour);


                            ConstraintLayout.LayoutParams cpuConstraints = (ConstraintLayout.LayoutParams) cpuTemp.getLayoutParams();
                            cpuConstraints.topMargin = cpuYFromTop;
                            Log.d("Monitor-LayoutConfig", "cpu: " + cpuConstraints.topToTop);
                            cpuTemp.requestLayout();
                            cpuTemp.setTextColor(textColour);
                        } catch (Throwable e){
                            Log.e("Monitor-LayoutConfig", e.getMessage());
                        }
                    }
                });
            } catch (Throwable e){
                Log.e("Monitor-LayoutConfig", e.getMessage());
            }
        }

        private int parseInt(String hex){
            String upperNibble = new StringBuilder().append(hex.charAt(0)).toString();
            String rest = hex.substring(1);
            int sign = 0b1000 & Integer.parseInt(upperNibble, 16);
            String unsigned = new StringBuilder()
                    .append(Integer.toString(0b0111 & Integer.parseInt(upperNibble, 16), 16))
                    .append(rest)
                    .toString();

            if(sign != 0){
                // is Negative
                return Integer.parseInt(unsigned, 16) | 0b10000000000000000000000000000000;
            } else {
                // is Positive
                return Integer.parseInt(unsigned);
            }
        }

        private String convertByteArrayToString(byte[] data){
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < data.length; i++){
                sb.append((char) data[i]);
            }
            return sb.toString();
        }

        private void staticBackgroundImage(ReceivedData data){
            final Drawable backgroundImage = Drawable.createFromPath(data.getPath());
            runOnUiThread(new Runnable() {
                    @Override
                    public void run() { background.setBackground(backgroundImage);
                    gifImageView.setVisibility(View.GONE); }
                });
        }

        private File writeToFile(byte[] data, String fileEnding) throws IOException{
            File outputFile = File.createTempFile("TransmittedBackgroundImage", fileEnding);
            if (!outputFile.createNewFile()) {
                outputFile.delete();
                outputFile.createNewFile();
            }
            FileOutputStream writer = new FileOutputStream(outputFile);
            writer.write(data);
            writer.close();
            return outputFile;
        }

        private void gifBackgroundImage(ReceivedData data){
            try {
                final GifDrawable gifDrawable = new GifDrawable(data.getPath());
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        gifImageView.setBackground(gifDrawable);

                        gifImageView.setVisibility(View.VISIBLE);
                    }
                });
            }catch(Exception e){
                Log.e("Monitor-Gif", e.getMessage());
            }
        }

        private byte[] deBoxByte(Byte[] data){
            byte[] r = new byte[data.length];
            for(int i = 0; i < data.length; i++){
                r[i] = data[i];
            }
            return r;
        }
    }

}
