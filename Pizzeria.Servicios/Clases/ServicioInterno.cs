using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Pizzeria.Servicios.Clases;

/// <summary>Servidor TCP que simula la cocina (puerto 5001) o el reparto (puerto 5002).</summary>
public class ServicioInterno
{
    private readonly string _rol;
    private readonly int _puerto;

    public ServicioInterno(string rol)
    {
        _rol = rol.ToLower();
        _puerto = _rol == "reparto" ? 5002 : 5001;
    }

    public async Task IniciarAsync()
    {
        var servidor = new TcpListener(IPAddress.Loopback, _puerto);
        servidor.Start();
        Log($"Escuchando en el puerto {_puerto}. Ctrl+C para apagar.");

        while (true)
        {
            var conexion = await servidor.AcceptTcpClientAsync();
            _ = Task.Run(() => AtenderAsync(conexion));   // cada pedido se atiende en paralelo
        }
    }

    private void Log(string texto) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{_rol.ToUpper()}] {texto}");

    private async Task AtenderAsync(TcpClient conexion)
    {
        using (conexion)
        {
            try
            {
                var stream = conexion.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // Protocolo de texto: COMANDO|idPedido|dato
                var partes = (await reader.ReadLineAsync() ?? "").Split('|');
                var id = partes.Length > 1 ? partes[1] : "?";
                var dato = partes.Length > 2 ? partes[2] : "";

                switch (partes[0])
                {
                    case "PREPARAR" when _rol == "cocina":
                        int cantidad = int.TryParse(dato, out var c) ? c : 1;
                        Log($"Pedido #{id}: {cantidad} pizza(s) al horno");
                        await writer.WriteLineAsync($"ACEPTADO|{id}");
                        await Task.Delay(2000 * cantidad + 1000);   // simula la cocción
                        await writer.WriteLineAsync($"LISTO|{id}");
                        Log($"Pedido #{id}: listo");
                        break;

                    case "ENTREGAR" when _rol == "reparto":
                        Log($"Pedido #{id}: sale hacia {dato}");
                        await writer.WriteLineAsync($"SALIO|{id}");
                        await Task.Delay(6000);                     // simula el viaje
                        await writer.WriteLineAsync($"ENTREGADO|{id}");
                        Log($"Pedido #{id}: entregado");
                        break;

                    default:
                        await writer.WriteLineAsync("ERROR|Mensaje no reconocido");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"Error atendiendo la conexión: {ex.Message}");
            }
        }
    }
}
