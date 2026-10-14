-- APIr Jordan - consultas de prueba y de reportes
-- Ejecutar DESPUÉS de schema.sql e inserts.sql. Cada bloque se puede correr por separado.
-- Las consultas 6 a 11 son exactamente los números que muestra el Panel de la aplicación
-- (Controllers/PanelController.cs), así se puede comparar la pantalla contra la base.

USE apirjordan;

-- ---------------------------------------------------------------
-- 1) Catálogo publicado con su stock total y cuántos talles tienen stock
-- ---------------------------------------------------------------
SELECT z.Id,
       z.Nombre,
       z.Colorway,
       z.Categoria,
       z.Precio,
       COALESCE(SUM(s.Cantidad), 0)                    AS StockTotal,
       SUM(CASE WHEN s.Cantidad > 0 THEN 1 ELSE 0 END) AS TallesConStock
FROM Zapatillas z
LEFT JOIN StockTalles s ON s.ZapatillaId = z.Id
WHERE z.Activa = 1
GROUP BY z.Id, z.Nombre, z.Colorway, z.Categoria, z.Precio
ORDER BY z.Categoria, z.Nombre;

-- ---------------------------------------------------------------
-- 2) Curva de talles de un modelo (lo que ve el cliente en la ficha)
-- ---------------------------------------------------------------
SELECT s.Talle,
       s.Cantidad,
       CASE
           WHEN s.Cantidad = 0 THEN 'agotado'
           WHEN s.Cantidad <= 2 THEN 'últimos pares'
           ELSE 'disponible'
       END AS Situacion
FROM StockTalles s
WHERE s.ZapatillaId = 1              -- Air Jordan 1 High "Chicago"
ORDER BY s.Talle;

-- ---------------------------------------------------------------
-- 3) Filtro del catálogo: modelos que tienen stock en un talle puntual (talle 42)
-- ---------------------------------------------------------------
SELECT z.Nombre, z.Colorway, z.Precio, s.Cantidad AS StockTalle42
FROM Zapatillas z
INNER JOIN StockTalles s ON s.ZapatillaId = z.Id
WHERE z.Activa = 1
  AND s.Talle = 42.0
  AND s.Cantidad > 0
ORDER BY z.Precio;

-- ---------------------------------------------------------------
-- 4) Pedidos con su cliente (lo que lista /Pedidos)
-- ---------------------------------------------------------------
SELECT p.Id        AS Pedido,
       p.CreadoEn,
       c.Nombre    AS Cliente,
       c.Email,
       p.Estado,
       p.Total,
       (SELECT SUM(i.Cantidad) FROM ItemsPedido i WHERE i.PedidoId = p.Id) AS Pares
FROM Pedidos p
INNER JOIN Clientes c ON c.Id = p.ClienteId
ORDER BY p.CreadoEn DESC;

-- ---------------------------------------------------------------
-- 5) Detalle de un pedido: líneas con modelo, talle y subtotal
-- ---------------------------------------------------------------
SELECT i.Id,
       z.Nombre,
       z.Colorway,
       i.Talle,
       i.Cantidad,
       i.PrecioUnitario,
       i.Cantidad * i.PrecioUnitario AS Subtotal
FROM ItemsPedido i
INNER JOIN Zapatillas z ON z.Id = i.ZapatillaId
WHERE i.PedidoId = 4
ORDER BY i.Id;

-- ---------------------------------------------------------------
-- 6) KPIs del panel: ventas, pedidos y ticket promedio (los cancelados no cuentan)
-- ---------------------------------------------------------------
SELECT COUNT(*)                                   AS PedidosValidos,
       SUM(p.Total)                               AS VentasTotales,
       ROUND(AVG(p.Total), 2)                     AS TicketPromedio,
       SUM(CASE WHEN p.Estado IN ('pendiente', 'pagado', 'enviado') THEN 1 ELSE 0 END) AS PedidosEnCurso
FROM Pedidos p
WHERE p.Estado <> 'cancelado';

-- ---------------------------------------------------------------
-- 7) Pedidos por estado (resumen del panel y pestañas de /Pedidos)
-- ---------------------------------------------------------------
SELECT p.Estado, COUNT(*) AS Cantidad, SUM(p.Total) AS Monto
FROM Pedidos p
GROUP BY p.Estado
ORDER BY FIELD(p.Estado, 'pendiente', 'pagado', 'enviado', 'entregado', 'cancelado');

-- ---------------------------------------------------------------
-- 8) Modelos más vendidos (pares), excluyendo pedidos cancelados
-- ---------------------------------------------------------------
SELECT z.Nombre,
       z.Colorway,
       SUM(i.Cantidad)                  AS ParesVendidos,
       SUM(i.Cantidad * i.PrecioUnitario) AS Facturado
FROM ItemsPedido i
INNER JOIN Pedidos p    ON p.Id = i.PedidoId
INNER JOIN Zapatillas z ON z.Id = i.ZapatillaId
WHERE p.Estado <> 'cancelado'
GROUP BY z.Id, z.Nombre, z.Colorway
ORDER BY ParesVendidos DESC, Facturado DESC
LIMIT 5;

-- ---------------------------------------------------------------
-- 9) Talles más vendidos (sirve para decidir qué reponer)
-- ---------------------------------------------------------------
SELECT i.Talle, SUM(i.Cantidad) AS ParesVendidos
FROM ItemsPedido i
INNER JOIN Pedidos p ON p.Id = i.PedidoId
WHERE p.Estado <> 'cancelado'
GROUP BY i.Talle
ORDER BY ParesVendidos DESC, i.Talle;

-- ---------------------------------------------------------------
-- 10) Ventas por categoría
-- ---------------------------------------------------------------
SELECT z.Categoria,
       SUM(i.Cantidad)                    AS Pares,
       SUM(i.Cantidad * i.PrecioUnitario) AS Monto
FROM ItemsPedido i
INNER JOIN Pedidos p    ON p.Id = i.PedidoId
INNER JOIN Zapatillas z ON z.Id = i.ZapatillaId
WHERE p.Estado <> 'cancelado'
GROUP BY z.Categoria
ORDER BY Monto DESC;

-- ---------------------------------------------------------------
-- 11) Stock bajo: talles con 1 o 2 pares en modelos publicados (alerta del panel)
-- ---------------------------------------------------------------
SELECT z.Nombre, z.Colorway, s.Talle, s.Cantidad
FROM StockTalles s
INNER JOIN Zapatillas z ON z.Id = s.ZapatillaId
WHERE z.Activa = 1
  AND s.Cantidad BETWEEN 1 AND 2
ORDER BY s.Cantidad, z.Nombre, s.Talle;

-- ---------------------------------------------------------------
-- 12) Modelos agotados (publicados pero sin stock en ningún talle)
-- ---------------------------------------------------------------
SELECT z.Id, z.Nombre, z.Colorway
FROM Zapatillas z
WHERE z.Activa = 1
  AND NOT EXISTS (SELECT 1 FROM StockTalles s WHERE s.ZapatillaId = z.Id AND s.Cantidad > 0);

-- ---------------------------------------------------------------
-- 13) Clientes con cantidad de pedidos y total gastado (lo que lista /Clientes)
-- ---------------------------------------------------------------
SELECT c.Id,
       c.Nombre,
       c.Email,
       COUNT(p.Id)                                                        AS Pedidos,
       COALESCE(SUM(CASE WHEN p.Estado <> 'cancelado' THEN p.Total END), 0) AS TotalGastado,
       MAX(p.CreadoEn)                                                    AS UltimoPedido
FROM Clientes c
LEFT JOIN Pedidos p ON p.ClienteId = c.Id
GROUP BY c.Id, c.Nombre, c.Email
ORDER BY TotalGastado DESC;

-- ---------------------------------------------------------------
-- 14) Control de integridad: el Total guardado debe coincidir con la suma de sus líneas
--     (si esta consulta devuelve filas, hay un pedido inconsistente)
-- ---------------------------------------------------------------
SELECT p.Id, p.Total, SUM(i.Cantidad * i.PrecioUnitario) AS TotalCalculado
FROM Pedidos p
INNER JOIN ItemsPedido i ON i.PedidoId = p.Id
GROUP BY p.Id, p.Total
HAVING p.Total <> SUM(i.Cantidad * i.PrecioUnitario);

-- ---------------------------------------------------------------
-- 15) Cómo descuenta stock la aplicación al confirmar un pedido (misma lógica que
--     PedidosController.Checkout, acá a mano). El UPDATE lleva la condición "Cantidad >= pares":
--     si no alcanza, afecta 0 filas y la app cancela toda la operación.
--     Se deja con ROLLBACK para poder ejecutarlo sin modificar los datos de ejemplo.
-- ---------------------------------------------------------------
START TRANSACTION;

UPDATE StockTalles
SET Cantidad = Cantidad - 1
WHERE ZapatillaId = 5 AND Talle = 40.0 AND Cantidad >= 1;      -- AJ1 Low Bred Toe, talle 40

INSERT INTO Pedidos (ClienteId, Estado, Total, Nota)
VALUES (2, 'pendiente', 249999.00, 'Prueba manual desde consultas.sql');

INSERT INTO ItemsPedido (PedidoId, ZapatillaId, Talle, Cantidad, PrecioUnitario)
VALUES (LAST_INSERT_ID(), 5, 40.0, 1, 249999.00);

SELECT Cantidad AS StockDespuesDelDescuento FROM StockTalles WHERE ZapatillaId = 5 AND Talle = 40.0;

ROLLBACK;   -- cambiar por COMMIT para dejar el pedido guardado

-- ---------------------------------------------------------------
-- 16) Cancelar un pedido devolviendo el stock (lo que hace CambiarEstado -> cancelado)
--     También dentro de una transacción con ROLLBACK para no alterar los datos.
-- ---------------------------------------------------------------
START TRANSACTION;

UPDATE StockTalles s
INNER JOIN ItemsPedido i ON i.ZapatillaId = s.ZapatillaId AND i.Talle = s.Talle
SET s.Cantidad = s.Cantidad + i.Cantidad
WHERE i.PedidoId = 4;

UPDATE Pedidos SET Estado = 'cancelado', ActualizadoEn = NOW() WHERE Id = 4 AND Estado IN ('pendiente', 'pagado');

SELECT z.Nombre, s.Talle, s.Cantidad
FROM StockTalles s
INNER JOIN Zapatillas z ON z.Id = s.ZapatillaId
WHERE (s.ZapatillaId, s.Talle) IN (SELECT i.ZapatillaId, i.Talle FROM ItemsPedido i WHERE i.PedidoId = 4);

ROLLBACK;
