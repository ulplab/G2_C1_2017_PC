﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using ClasesComunicacion;

namespace JuegoAhorcado
{
    public delegate void enviar(string Json);
    public delegate void enviaFrmJuegoAcerto(clsMensajeBase msj);
    public delegate void enviaFrmTime(clsMensajeBase msj);
    public delegate void enviaFrmJuegoFallo();
    public delegate void comienzo(clsMensajeBase msj);
    public delegate void problemasConServidor(string mensaje,string mensajeformulario);
   


    public class clsCliente
    {
        //clsJugador player;
        TcpClient client;
        static int port = 8000;
        clsMensajeBase mensaje;
        private String nick;
        public String Nick
        {
            get { return nick; }
            set { nick = value; }
        }
        public clsMensajeBase Mensaje
        {
            get { return mensaje; }
            set { mensaje = value; }
        }

        clsManejoPaquetes serializador = new clsManejoPaquetes();
        static private NetworkStream stream;
        static private StreamWriter streamw;
        static private StreamReader streamr;
        public event comienzo start;
        public event enviaFrmJuegoAcerto acertoLetra;
        public event enviaFrmJuegoFallo falloLetra;
        public event enviaFrmJuegoAcerto acertoPalabra;
        public event enviaFrmJuegoFallo falloPalabra;
        public event enviaFrmTime timeForm;
        public event problemasConServidor DesconexionServidor;//mandamos un mensaje al formulario que tenga problemas con la desconieccion con el servidor

        public void leer()
        {
            try
            {
                while (true)
                {
                    string aux = streamr.ReadLine();
                    mensaje = serializador.recibirMensaje(aux);
                    switch (mensaje.Tipo)
                    {
                        case "MENSAJE_JUEGO":
                            {
                                clsMensajeJuego mensajeJuego = (clsMensajeJuego)mensaje;
                                if (mensaje.Retorno != "FALLO" && mensaje.Accion == "PROBAR_LETRA")
                                    acertoLetra(mensajeJuego);
                                else if (mensaje.Retorno == "FALLO" && mensaje.Accion == "PROBAR_LETRA")
                                    falloLetra();

                                else if (mensaje.Retorno == "FALLO" && mensaje.Accion == "PROBAR_PALABRA")
                                    falloPalabra();
                            } break;

                        case "MENSAJE_PERDEDOR":
                            {
                                falloPalabra();
                            } break;
                        case "MENSAJE_GANADOR":
                            {
                                clsMensajeGanador mensajeGanador = (clsMensajeGanador)mensaje;
                                acertoPalabra(mensajeGanador);
                            } break;
                        case "MENSAJE_TIMER":
                            {
                                clsMensajeTimer mensajeTimer = (clsMensajeTimer)mensaje;
                                timeForm(mensajeTimer);
                                if (mensajeTimer.Segundero == 0)
                                    falloPalabra();
                            } break;
                    }
                }
            }
            catch
            {
                DesconexionServidor("!Error de conexión con el servidor", "Upss!!");
            }
           
        }
        public void Iniciar()
        {
            try
            {
                client = new TcpClient("127.0.0.1", port);
                Console.WriteLine("Client conectado.");
                stream = client.GetStream();
                streamw = new StreamWriter(stream);
                streamr = new StreamReader(stream);
                clsMensajeBase msjNick = new clsMensajeBase();
                msjNick.Nick = nick;
                streamw.WriteLine(serializador.enviarMensaje(msjNick));
                streamw.Flush();


                string aux = streamr.ReadLine();
                mensaje = serializador.recibirMensaje(aux);
                start(mensaje);
                Thread t = new Thread(leer);
                t.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex + "Client Disconnected.");
            }
            catch (IOException )
            {
                DesconexionServidor("!Error de conexión con el servidor","Upss!!");
            }
        }
 
        public void enviar(clsMensajeBase msj)
        {
            try
            {
                string aux = serializador.enviarMensaje(msj);
                streamw.WriteLine(aux);
                streamw.Flush();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

    }
}
