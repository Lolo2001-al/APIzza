using System.Net.Sockets;

namespace Pizzeria.Api.Clases;

/// <summary>Delega cada pedido a la cocina y al reparto por sockets, con async/await y manejo de errores.</summary>
public static class Despachador
{
    public static async Task ProcesarAsync(Pedido pedido, ILogger log)
    {
        try
        {
            int cantidad = pedido.Lineas.Sum(l => l.Cantidad);

            // 1) Cocina: ACEPTADO -> EnPreparacion, LISTO -> sigue el reparto
            await HablarAsync("Cocina", 5001, $"PREPARAR|{pedido.Id}|{cantidad}", r =>
            {
                if (r.StartsWith("ACEPTADO")) pedido.Estado = EstadoPedido.EnPreparacion;
            }, log);

            // 2) Reparto: SALIO -> EnViaje, ENTREGADO -> Entregado
            var direccion = pedido.Cliente.Direccion.Replace('|', ' ');
            await HablarAsync("Reparto", 5002, $"ENTREGAR|{pedido.Id}|{direccion}", r =>
            {
                if (r.StartsWith("SALIO")) pedido.Estado = EstadoPedido.EnViaje;
                if (r.StartsWith("ENTREGADO")) pedido.Estado = EstadoPedido.Entregado;
            }, log);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Pedido #{Id} cancelado", pedido.Id);
            pedido.Estado = EstadoPedido.Cancelado;
            pedido.Detalle = "Tuvimos un problema con la cocina o el reparto y cancelamos tu pedido. Probá de nuevo en unos minutos.";
        }
    }

    // Abre un socket, manda un mensaje y lee 2 respuestas. Reintenta hasta 3 veces si la red falla.
    static async Task HablarAsync(string servicio, int puerto, string mensaje, Action<string> alRecibir, ILogger log)
    {
        const int intentos = 3;
        for (int i = 1; i <= intentos; i++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                using var tcp = new TcpClient();
                await tcp.ConnectAsync("127.0.0.1", puerto, cts.Token);
                var stream = tcp.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };

                await writer.WriteLineAsync(mensaje);
                for (int n = 0; n < 2; n++)
                {
                    var r = await reader.ReadLineAsync(cts.Token)
                            ?? throw new IOException($"{servicio} cerró la conexión");
                    log.LogInformation("{Servicio} respondió: {Respuesta}", servicio, r);
                    if (r.StartsWith("ERROR")) throw new InvalidOperationException(r);
                    alRecibir(r);
                }
                return;
            }
            catch (Exception ex) when ((ex is SocketException or IOException or OperationCanceledException) && i < intentos)
            {
                log.LogWarning("{Servicio} no respondió (intento {I}/{Max}): {Msg}", servicio, i, intentos, ex.Message);
                await Task.Delay(1000 * i);
            }
        }
    }
}
