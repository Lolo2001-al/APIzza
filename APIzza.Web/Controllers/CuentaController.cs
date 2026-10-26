using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIzza.Web.Data;
using APIzza.Web.Models;
using APIzza.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APIzza.Web.Controllers;

public class CuentaController : Controller
{
    private const string CookieJwt = "APIzza.Auth";
    private readonly ApizzaDbContext _contexto;
    private readonly IConfiguration _configuracion;
    private readonly PasswordHasher<Cliente> _hasher = new();

    public CuentaController(ApizzaDbContext contexto, IConfiguration configuracion)
    {
        _contexto = contexto;
        _configuracion = configuracion;
    }

    [HttpGet]
    public IActionResult Registrar() => View(new RegistroViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        string email = modelo.Email.Trim().ToLowerInvariant();
        if (await _contexto.Clientes.AnyAsync(c => c.Email == email))
        {
            ModelState.AddModelError(nameof(modelo.Email), "Ya existe una cuenta con ese correo.");
            return View(modelo);
        }

        var cliente = new Cliente
        {
            Nombre = modelo.Nombre.Trim(),
            Email = email,
            Direccion = ""
        };
        cliente.PasswordHash = _hasher.HashPassword(cliente, modelo.Contrasena);
        _contexto.Clientes.Add(cliente);
        await _contexto.SaveChangesAsync();
        CrearCookieJwt(cliente);
        TempData["Mensaje"] = "¡Cuenta creada! Ya iniciaste sesión.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult IniciarSesion(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(LoginViewModel modelo, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(modelo);
        }

        string email = modelo.Email.Trim().ToLowerInvariant();
        var cliente = await _contexto.Clientes.FirstOrDefaultAsync(c => c.Email == email);
        if (cliente?.PasswordHash == null ||
            _hasher.VerifyHashedPassword(cliente, cliente.PasswordHash, modelo.Contrasena) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "El correo o la contraseña son incorrectos.");
            ViewBag.ReturnUrl = returnUrl;
            return View(modelo);
        }

        CrearCookieJwt(cliente);
        TempData["Mensaje"] = "¡Iniciaste sesión correctamente!";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CerrarSesion()
    {
        Response.Cookies.Delete(CookieJwt);
        TempData["Mensaje"] = "Cerraste sesión.";
        return RedirectToAction("Index", "Home");
    }

    private void CrearCookieJwt(Cliente cliente)
    {
        string clave = _configuracion["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Key.");
        var seguridad = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
        var credenciales = new SigningCredentials(seguridad, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
            new Claim(ClaimTypes.Name, cliente.Nombre),
            new Claim(ClaimTypes.Email, cliente.Email)
        };
        var token = new JwtSecurityToken(
            issuer: _configuracion["Jwt:Issuer"],
            audience: _configuracion["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credenciales);

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);
        Response.Cookies.Append(CookieJwt, jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(2),
            IsEssential = true
        });
    }
}
