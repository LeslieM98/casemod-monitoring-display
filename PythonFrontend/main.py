#! /bin/python3
import struct
import threading
import socket
import sys
import json
import os
import _thread
import time

from datetime import datetime

from kivy.app import App
from kivy.clock import Clock, mainthread
from kivy.properties import StringProperty, NumericProperty, ColorProperty
from kivy.logger import Logger
from kivy.uix.floatlayout import FloatLayout

hostname = 'raspberrypi'
def recv_all(sock):
    BUFF_SIZE = 16384
    data = b''
    while True:
        part = sock.recv(BUFF_SIZE)
        Logger.debug("Network: Received {0} bytes".format(len(part)))
        data += part
        if not part:
            break
    Logger.info("Network: {1} Received {0} bytes".format(len(data), datetime.now().strftime("%H:%M:%S")))
    return data

class MainView(FloatLayout):
    cpu_temp = StringProperty()
    gpu_temp = StringProperty()

    text_colour = ColorProperty()
    gpu_y_from_bottom = NumericProperty()
    cpu_y_from_top = NumericProperty()
    gif_path = StringProperty()

    def __init__(self):
        super(MainView, self).__init__()
        self.change_cpu_temp(-12)
        self.change_gpu_temp(-12)
        self.change_cpu_y_from_top(0.1)
        self.change_gpu_y_from_bottom(0.1)
        self.change_gif_path('received.gif')

    @mainthread
    def change_gpu_temp(self, temp_in_celsius):
        self.gpu_temp = '[size=40]GPU: {0}°C[/size]'.format(temp_in_celsius)

    @mainthread
    def change_cpu_temp(self, temp_in_celsius):
        self.cpu_temp = '[size=40]CPU: {0}°C[/size]'.format(temp_in_celsius)

    @mainthread
    def change_text_colour(self, value):
        self.text_colour = value
        Logger.info('Layout: changed colour to {0}'.format(value))

    @mainthread
    def change_gpu_y_from_bottom(self, value):
        self.gpu_y_from_bottom = value
        Logger.info('Layout: changed gpu_y_from_bottom to {0}'.format(value))

    @mainthread
    def change_cpu_y_from_top(self, value):
        self.cpu_y_from_top = 1 - value
        Logger.info('Layout: changed cpu_y_from_top to {0}'.format(value))

    @mainthread
    def change_gif_path(self, new_path):
        self.gif_path = new_path
        self.ids['GIF_DISPLAY'].reload()
        Logger.info('Layout: changed gif_path to {0}'.format(new_path))


class TemperatureMonitorApp(App):
    def __init__(self):
        super(TemperatureMonitorApp, self).__init__()

    @mainthread
    def change_gpu_temp(self, val):
        self.view.change_gpu_temp(val)

    @mainthread
    def change_cpu_temp(self, val):
        self.view.change_cpu_temp(val)

    @mainthread
    def change_text_colour(self, val):
        self.view.change_text_colour(val)

    @mainthread
    def change_gpu_y_from_bottom(self, val):
        self.view.change_gpu_y_from_bottom(val)

    @mainthread
    def change_cpu_y_from_top(self, val):
        self.view.change_cpu_y_from_top(val)

    @mainthread
    def change_gif_path(self, val):
        self.view.change_gif_path(val)

    def build(self):
        view = MainView()
        self.view = view
        return view


class TemperatureThread(threading.Thread):
    def __init__(self, change_cpu_temp, change_gpu_temp, port):
        threading.Thread.__init__(self)

        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        server_address = (hostname, port)
        Logger.info('Network: Temperature service starting up on {} port {}'.format(*server_address))
        sock.bind(server_address)

        self.sock = sock
        self.change_cpu_temp = change_cpu_temp
        self.change_gpu_temp = change_gpu_temp
        self.enabled = True

    def terminate(self):
        self.enabled = False
        self.sock.close()

    def run(self):
        while self.enabled:
            try:
                data, _ = self.sock.recvfrom(32)
                Logger.debug(f'Network: {datetime.now().strftime("%H:%M:%S")}: Received temperature package')
                cpu_temp = struct.unpack('f', data[:4])[0]
                gpu_temp = struct.unpack('f', data[4:])[0]

                self.change_cpu_temp(cpu_temp)
                self.change_gpu_temp(gpu_temp)
            except AttributeError:
                pass



class ConfigurationThread(threading.Thread):

    def __init__(self, set_text_colour, set_gpu_y_from_bottom, set_cpu_y_from_top, set_gif_path, port):
        threading.Thread.__init__(self)

        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_address = (hostname, port)
        sock.bind(server_address)
        sock.listen(2)
        Logger.info('Network: Configuration service starting up on {} port {}'.format(*server_address))

        self.sock = sock
        self.enabled = True
        self.set_text_colour = set_text_colour
        self.set_gpu_y_from_bottom = set_gpu_y_from_bottom
        self.set_cpu_y_from_top = set_cpu_y_from_top
        self.set_gif_path = set_gif_path

    def run(self):
        while self.enabled:
            connection, _ = self.sock.accept()
            _thread.start_new_thread(self.worker, (connection, ))

    def worker(self, connection: socket.socket):
        Logger.info("Configuration: Started worker")
        payload_type = int.from_bytes(connection.recv(1), 'little')
        payload = recv_all(connection)
        self.dispatch_payload(payload_type, payload)

    def dispatch_payload(self, payload_type, payload):
        payload_layout_configuration = 0
        payload_static_picture = 1
        payload_gif = 2

        if payload_type == payload_layout_configuration:
            self.layout_payload(payload)
        elif payload_type == payload_static_picture:
            self.static_pic_payload(payload)
        elif payload_type == payload_gif:
            self.gif_payload(payload)

    def layout_payload(self, payload):
        decoded_payload = payload.decode('ascii')
        json_data = json.loads(decoded_payload)

        gpu_y_from_bottom = json_data['gpu_y_from_bottom']
        cpu_y_from_top = json_data['cpu_y_from_top']
        text_colour = json_data['text_colour']

        self.set_gpu_y_from_bottom(gpu_y_from_bottom)
        self.set_cpu_y_from_top(cpu_y_from_top)
        self.set_text_colour(text_colour)

    def gif_payload(self, payload):
        gif_path = 'received.gif'

        if os.path.exists(gif_path):
            os.remove(gif_path)
        gif_file = open(gif_path, 'wb')
        gif_file.write(payload)
        gif_file.close()

        Clock.schedule_once(lambda dt: self.set_gif_path(gif_path))

    @staticmethod
    def static_pic_payload(payload):
        Logger.error("Configuration: Static images not supported yet")

    def terminate(self):
        self.enabled = False
        self.sock.close()


if __name__ == "__main__":
    main_view = TemperatureMonitorApp()
    port = 8080
    temperature_thread = TemperatureThread(main_view.change_cpu_temp,
                                           main_view.change_gpu_temp,
                                           port)
    configuration_thread = ConfigurationThread(main_view.change_text_colour,
                                               main_view.change_gpu_y_from_bottom,
                                               main_view.change_cpu_y_from_top,
                                               main_view.change_gif_path,
                                               port)

    configuration_thread.start()
    temperature_thread.start()
    main_view.run()

    temperature_thread.terminate()
    configuration_thread.terminate()
    sys.exit(0)
