package me.leslie.monitor.monitorapp;

import android.bluetooth.le.ScanCallback;
import android.renderscript.ScriptGroup;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public class ReceivedData {
    private final File cache;
    private final int prefix;
    public ReceivedData(InputStream input) throws IOException{
        cache = File.createTempFile("ReceivedMonitoringConfiguration", ".tmp");
        prefix = input.read();
        FileOutputStream outputStream = new FileOutputStream(cache);
        byte[] buf = new byte[512];
        int length;
        while((length = input.read(buf)) > 0){
            outputStream.write(buf, 0, length);
        }
        outputStream.close();
    }

    public byte getPrefix() throws IOException {
        return (byte) prefix;
    }

    public byte[] getData() throws IOException {
        InputStream is = new FileInputStream(cache);
        List<Byte> read = new ArrayList<>();
        is.read();
        int b;
        while((b = is.read()) >= 0){
            read.add((byte) b);
        }

        return deBoxByte(read.toArray(new Byte[0]));
    }

    public String getPath(){
        return cache.getAbsolutePath();
    }

    private byte[] deBoxByte(Byte[] data){
        byte[] r = new byte[data.length];
        for(int i = 0; i < data.length; i++){
            r[i] = data[i];
        }
        return r;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        ReceivedData that = (ReceivedData) o;
        return Objects.equals(cache, that.cache);
    }

    @Override
    public int hashCode() {
        return Objects.hash(cache);
    }
}
