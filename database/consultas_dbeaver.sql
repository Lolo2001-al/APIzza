-- APIzza - consultas para VER los datos en DBeaver
-- Ejecutar una por una (seleccionar la consulta y Ctrl+Enter)

USE apizza;

-- 1) Menú completo de pizzas (con precios y categoría)
SELECT Id, Nombre, Precio, Categoria, Descripcion
FROM Pizzas
ORDER BY Categoria, Nombre;

-- 2) Clientes que hicieron pedidos
SELECT * FROM Clientes;

-- 3) Pedidos con los datos del cliente
SELECT p.Id AS Pedido, c.Nombre, c.Email, c.Telefono, c.Direccion,
       p.Estado, p.Total, p.CreadoEn
FROM Pedidos p
JOIN Clientes c ON c.Id = p.ClienteId
ORDER BY p.CreadoEn DESC;

-- 4) Detalle completo: quién pidió qué pizza y cuántas
SELECT p.Id AS Pedido, c.Nombre AS Cliente, pz.Nombre AS Pizza,
       i.Cantidad, i.PrecioUnitario,
       (i.Cantidad * i.PrecioUnitario) AS Subtotal,
       p.Total, p.CreadoEn
FROM ItemsPedido i
JOIN Pedidos  p  ON p.Id  = i.PedidoId
JOIN Clientes c  ON c.Id  = p.ClienteId
JOIN Pizzas   pz ON pz.Id = i.PizzaId
ORDER BY p.CreadoEn DESC, p.Id;

-- 5) Pizzas agregadas desde la web, por categoría
SELECT Categoria, COUNT(*) AS Cantidad FROM Pizzas GROUP BY Categoria;

-- 6) (Opcional) Agregar una pizza a mano por SQL, en su categoría
-- Categorías válidas: 'clasica', 'especial', 'vegetariana'
-- INSERT INTO Pizzas (Nombre, Descripcion, Precio, Categoria)
-- VALUES ('Calabresa', 'Muzzarella y longaniza', 8000.00, 'especial');
