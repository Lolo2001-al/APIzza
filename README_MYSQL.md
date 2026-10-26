# APIzza + MySQL

## 1. Requisito

Instalá MySQL Server 8.x o superior y asegurate de que el servicio esté iniciado. No necesitás XAMPP ni Docker. El proyecto usa el proveedor oficial `MySql.EntityFrameworkCore` 10.0.9, compatible con EF Core 10/net10.0.

## 2. Crear la base

Abrí MySQL Workbench o la consola de MySQL y ejecutá primero `database/schema.sql` y después `database/inserts.sql`. **No vuelvas a ejecutar `schema.sql` sobre una BD con datos importantes**, porque sus `DROP TABLE` borran las tablas y sus datos.

## 3. Conexión de la web

Archivo: `APIzza.Web/appsettings.json`

La conexión preparada es:

```text
Server=127.0.0.1;Port=3306;Database=apizza;User=root;Password=CAMBIAR_ESTO;SslMode=None;
```

- `Server=127.0.0.1`: MySQL está en tu propia PC (equivale a `localhost`).
- `Port=3306`: puerto habitual de MySQL.
- `Database=apizza`: base creada por `schema.sql`.
- `User=root`: usuario de MySQL.
- `Password=...`: reemplazá `CAMBIAR_ESTO` por la contraseña que configuraste al instalar MySQL.

Si tu MySQL usa otro puerto, por ejemplo 3307, cambialo en esa misma cadena.

## 4. Ejecutar la página

Desde la carpeta que contiene `APIzza.Web.csproj`:

```powershell
cd .\APIzza.Web
dotnet restore
dotnet run
```

La web mostrará una URL como `http://localhost:xxxx`. **Ese `localhost` es el servidor web de ASP.NET; la base de datos está en `127.0.0.1:3306` dentro de MySQL.**

## 5. Qué queda guardado

- Al confirmar un pedido se guarda/actualiza el cliente en `Clientes`.
- Se crea el pedido en `Pedidos`.
- Cada pizza y cantidad del pedido se guarda en `ItemsPedido`.
- Las pizzas nuevas cargadas desde **Agregar pizza** se guardan en `Pizzas` y aparecen en la categoría seleccionada.
