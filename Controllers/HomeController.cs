using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RopaMexicana_171G0250_171G0222.Helpers;
using RopaMexicana_171G0250_171G0222.Models;
using RopaMexicana_171G0250_171G0222.Models.ViewModels;
using RopaMexicana_171G0250_171G0222.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RopaMexicana_171G0250_171G0222.Controllers
{
    public class HomeController : Controller
    {
        public IWebHostEnvironment Environment { get; set; }
        public HomeController(IWebHostEnvironment env)
        {
            Environment = env;
        }

        public IActionResult Index()
        {
           
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            var afiliado = context.MarcaAfiliada.Include(x => x.Producto).OrderBy(x => x.Nombre).Select(x => new AfiliadosViewModel
            {
                MarcaAfiliado = x.Marca,
                Productos = x.Producto
            });
            return View(afiliado);
        }


            [AllowAnonymous]
        public IActionResult InicioDeSesionAdministrador()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> InicioDeSesionAdministrador(Administrador A)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            Repository<Administrador> repository = new Repository<Administrador>(context);
            var Administrador = context.Administrador.FirstOrDefault(x => x.Usuario == A.Usuario);
            try
            {
                if (Administrador != null && Administrador.Contraseña == A.Contraseña)
                {
                    List<Claim> info = new List<Claim>();
                    info.Add(new Claim(ClaimTypes.Name, "Us" + Administrador.Nombre));
                    info.Add(new Claim("Usuario", Administrador.Usuario));
                    info.Add(new Claim(ClaimTypes.Role, "Administrador"));
                    info.Add(new Claim("Nombre", Administrador.Nombre));

                    var ClaimsIdentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                    var ClaimsPrincipal = new ClaimsPrincipal(ClaimsIdentity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal, new AuthenticationProperties
                    { IsPersistent = true });
                    return RedirectToAction("SesionIniciada");
                }
                else
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                    return View(A);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(A);
            }
        }


        [AllowAnonymous]
        public async Task<IActionResult> CierreDeSesion()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Registro(MarcaAfiliada us, string contra, string confcontra)
        {
            sistem14_ropa_mexicanaContext Context = new sistem14_ropa_mexicanaContext();
            Repository<MarcaAfiliada> repos = new Repository<MarcaAfiliada>(Context);
            try
            {
                if (Context.MarcaAfiliada.Any(x => x.Correo == us.Correo))
                {
                    ModelState.AddModelError("", "Este correo se encuentra registrado");
                    return View(us);
                }
                else
                {
                    if (contra == confcontra)
                    {
                        us.Contrasena = HashingHelper.GetHash(contra);
                        us.ClaveAct = ClaveHelper.ClaveActivacion();
                        us.Activo = 0;
                        repos.Insert(us);

                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("sistemascomputacionales7g@gmail.com", "Ropa Mexicana");
                        message.To.Add(us.Correo);
                        message.Subject = "Correo de activación envíado";

                        string mensaje = System.IO.File.ReadAllText(Environment.WebRootPath + "/Clave.html");
                        message.Body = mensaje.Replace("##Clave##", us.ClaveAct.ToString());
                        message.IsBodyHtml = true;

                        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                        client.Send(message);
                        return RedirectToAction("ActivacionDeCuenta");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Las contraseñas no coinciden");
                        return View(us);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(us);
            }
        }


        public IActionResult ActivacionDeCuenta()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ActivacionDeCuenta(int clave)
        {
            sistem14_ropa_mexicanaContext Context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repos = new MarcaRepository(Context);
            var usuario = Context.MarcaAfiliada.FirstOrDefault(x => x.ClaveAct == clave);
            if (usuario.Activo == 0)
            {
                var cla = usuario.ClaveAct;
                if (clave == cla)
                {
                    usuario.Activo = 1;
                    repos.Update(usuario);
                    return RedirectToAction("InicioDeSesion");
                }
                else
                {
                    ModelState.AddModelError("", "Clave incorrecta.");
                    return View(clave);
                }

            }
            else
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View(clave);
            }
        }


        [AllowAnonymous]
        public IActionResult InicioDeSesion()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> InicioDeSesion(MarcaAfiliada us, bool recordar)
        {
            sistem14_ropa_mexicanaContext Context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repos = new MarcaRepository(Context);
            var usuario = repos.GetUsByCorreo(us.Correo);
            if (usuario != null && HashingHelper.GetHash(us.Contrasena) == usuario.Contrasena)
            {
                if (usuario.Activo == 1)
                {
                    List<Claim> info = new List<Claim>();
                    info.Add(new Claim(ClaimTypes.Name, "Us"+ usuario.Nombre));
                    info.Add(new Claim(ClaimTypes.Role, "UsuarioActivo"));
                    info.Add(new Claim(ClaimTypes.Role, "Afiliado"));
                    info.Add(new Claim("Nombre", usuario.Nombre));
                    info.Add(new Claim("Marca", usuario.Marca));
                    info.Add(new Claim("Correo electronico", usuario.Correo));
                    info.Add(new Claim("Id", usuario.Id.ToString()));
                    var claimidentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimprincipal = new ClaimsPrincipal(claimidentity);
                    if (recordar == true)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimprincipal, new AuthenticationProperties
                        { IsPersistent = true });
                    }
                    else
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimprincipal, new AuthenticationProperties
                        { IsPersistent = false });
                    }
                    return RedirectToAction("SesionIniciada");
                }
                else
                {
                    ModelState.AddModelError("", "Usuario no registrado");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("", "Correo electronico y/o  contraseña erroneos");
                return View();
            }
        }

       

        [Authorize (Roles = "Administrador, Afiliado")]
        public IActionResult SesionIniciada()
        {
            return View();
        }


        [Authorize(Roles = "Administrador")]
        public IActionResult VerAfiliados()
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
           MarcaRepository repository = new MarcaRepository(context);
            var ListaDeMarcas = repository.GetAll();
            return View(ListaDeMarcas);
        }



        [Authorize(Roles = "Administrador")]
        public IActionResult AñadirAfiliados()
        {
            return View();
        }




        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public IActionResult AñadirAfiliados(MarcaAfiliada MarcaAf)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            try
            {
                var verify = repository.GetAfiliadosByMarca(MarcaAf.Marca);
                if (verify != null)
                {
                    ModelState.AddModelError("", "Ya existe un afiliado con esta marca");
                    return View(MarcaAf);
                }
                else
                {
                    MarcaAf.Activo = 1;
                    MarcaAf.Contrasena = HashingHelper.GetHash(MarcaAf.Contrasena);
                    repository.Insert(MarcaAf);
                    return RedirectToAction("VerAfiliados");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(MarcaAf);
            }
        }



        [Authorize(Roles = "Administrador")]
        public IActionResult EditarDatosAfiliados(int id)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            var afiliado = repository.Get(id);
            if (afiliado == null)
            {
                return RedirectToAction("VerAfiliados");
            }
            return View(afiliado);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public IActionResult EditarDatosAfiliados(MarcaAfiliada MarcaAfiliado)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            var afiliado = repository.Get(MarcaAfiliado.Id);
            try
            {
                if (afiliado != null)
                {
                    afiliado.Nombre = MarcaAfiliado.Nombre;
                    afiliado.Marca = MarcaAfiliado.Marca;
                    afiliado.Correo = MarcaAfiliado.Correo;

                    repository.Update(afiliado);
                }
                return RedirectToAction("VerAfiliados");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(afiliado);
            }
        }



        [Authorize(Roles = "Administrador")]
        public IActionResult CambiarContraseñaAfiliados(int id)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            var afiliado = repository.Get(id);
            if (afiliado == null)
            {
                return RedirectToAction("VerAfiliados");
            }
            return View(afiliado);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public IActionResult CambiarContraseñaAfiliados(MarcaAfiliada m, string contraseña, string confcontraseña)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            var afiliado = repository.Get(m.Id);
            try
            {
                if (afiliado != null)
                {
                    if (contraseña == afiliado.Contrasena)
                    {
                        ModelState.AddModelError("", "La nueva contraseña no puedo ser igual a la actual.");
                        return View(afiliado);
                    }
                    else
                    {
                        if (contraseña == confcontraseña)
                        {
                            afiliado.Contrasena = contraseña;
                            afiliado.Contrasena = HashingHelper.GetHash(contraseña);
                            repository.Update(afiliado);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Las contraseñas no coinciden");
                            return View(afiliado);
                        }
                    }
                }
                return RedirectToAction("VerAfiliados");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(afiliado);
            }
        }



        [HttpPost]
        public IActionResult DesactivarAfiliado(MarcaAfiliada MA)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            var afiliado = repository.Get(MA.Id);
            if (afiliado != null && afiliado.Activo == 1)
            {
                afiliado.Activo = 0;
                repository.Update(afiliado);
            }
            else
            {
                afiliado.Activo = 1;
                repository.Update(afiliado);
            }
            return RedirectToAction("VerAfiliados");
        }


        [Authorize(Roles = "Administrador, Afiliado")]
        public IActionResult VerProductos(int id)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            var afiliado = repository.GetProductosById(id);
            if (afiliado != null)
            {
                return View(afiliado);
            }
            else
                return RedirectToAction("SesionIniciada");
        }


        [Authorize(Roles = "Administrador, Afiliado")]
        public IActionResult AñadirProducto(int id)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repository = new MarcaRepository(context);
            ProductoViewModel PVM = new ProductoViewModel();
            PVM.MarcaAfiliada = repository.Get(id);
            return View(PVM);
        }
        [Authorize(Roles = "Administrador, Afiliado")]
        [HttpPost]
        public IActionResult AñadirProducto(ProductoViewModel vm)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            MarcaRepository Mrepository = new MarcaRepository(context);
            ProductoRepository Prepository = new ProductoRepository(context);
            if (vm.Archivo.ContentType != "image/jpeg" || vm.Archivo.Length > 1024 * 1024 * 2)
            {
                ModelState.AddModelError("", "Debe selecionar un archivo jpg de menos de 2mb");
                 return View(vm);
            }
            try
            {
                var IdAfiliado = Mrepository.GetAfiliadosByMarca(vm.MarcaAfiliada.Marca).Id;
                vm.Producto.IdMarcaAfi = IdAfiliado;
                Prepository.Insert(vm.Producto);
                System.IO.FileStream fs = new FileStream(Environment.WebRootPath + "/imgs_Productos/" + vm.Producto.Id + "_0.jpg", FileMode.Create);
                vm.Archivo.CopyTo(fs);
                fs.Close();
                return RedirectToAction("VerProductos");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }



        [Authorize(Roles = "Administrador, Afiliado")]
        public IActionResult EditarDatosProducto(int id)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            ProductoViewModel vm = new ProductoViewModel();

            ProductoRepository pr = new ProductoRepository(context);
            vm.Producto = pr.Get(id);
            if (vm.Producto == null)
            {
                return RedirectToAction("Index");
            }
            MarcaRepository mrepository = new MarcaRepository(context);
            vm.MarcaAfiliadas = mrepository.GetAll();
            if (System.IO.File.Exists(Environment.WebRootPath + $"/imgs_Productos/{vm.Producto.Id}_0.jpg"))
            {
                vm.Imagen = vm.Producto.Id + "_0.jpg";
            }
            else
            {
                vm.Imagen = "NoPhoto.jpg";
            }

            return View(vm);
           
        }


        [Authorize(Roles = "Administrador, Afiliado")]
        [HttpPost]
        public IActionResult EditarDatosProducto(ProductoViewModel vm)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            if (vm.Archivo != null)
            {
                if (vm.Archivo.ContentType != "image/jpeg" || vm.Archivo.Length > 1024 * 1024 * 2)
                {
                    ModelState.AddModelError("", "Debe selecionar un archivo jpg de menos de 2mb");
                    MarcaRepository marcarepository = new MarcaRepository(context);
                    vm.MarcaAfiliadas = marcarepository.GetAll();

                    return View(vm);
                }
            }

            try
            {
                ProductoRepository repos = new ProductoRepository(context);

                var producto = repos.Get(vm.Producto.Id);
                if (producto != null)
                {
                    producto.Nombre = vm.Producto.Nombre;
                    producto.Costo = vm.Producto.Costo;
                    producto.Descripción = vm.Producto.Descripción;
                    producto.Color = vm.Producto.Color;
                    producto.Material = vm.Producto.Material;
                    producto.Hipervinculo = vm.Producto.Hipervinculo;
                    repos.Update(producto);
                    //Guardar archivo de inserción
                    if (vm.Archivo != null)
                    {
                        FileStream fs = new FileStream(Environment.WebRootPath + "/imgs_Productos/" + vm.Producto.Id + "_0.jpg", FileMode.Create);
                        vm.Archivo.CopyTo(fs);
                        fs.Close();
                    }

                }


                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
                MarcaRepository marcarepository = new MarcaRepository(context);
                vm.MarcaAfiliadas = marcarepository.GetAll();

                return View(vm);
            }
           
        }

        [Authorize(Roles = "Administrador, Afiliado")]
        [HttpPost]
        public IActionResult EliminarProducto(Producto p)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();
            ProductoRepository repository = new ProductoRepository(context);
            var producto = repository.Get(p.Id);
            if (producto != null)
            {
                repository.Delete(producto);
            }
            else
            {
                ModelState.AddModelError("", "Este producto no se encuentra registrado");
            }
            return RedirectToAction("VerProductos");
        }


        public IActionResult Detalles(string id)
        {
            sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext();

            ProductoRepository repos = new ProductoRepository(context);
            ProductoViewModel vm = new ProductoViewModel();
            vm.Producto = repos.GetProductosByNombre(id);

            if (vm.Producto == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
               
                return View(vm);
            }

        }


        [Route("{id}")]
        public IActionResult MarcaxProductos(string id)
        {
            using (sistem14_ropa_mexicanaContext context = new sistem14_ropa_mexicanaContext())
            {
                ProductoRepository repos = new ProductoRepository(context);
                AfiliadosViewModel vm = new AfiliadosViewModel();

                vm.MarcaAfiliado = id;
                vm.Productos = repos.GetProductosByAfiliados(id).ToList();


                return View(vm);
            }

        }


        [HttpPost]
        public IActionResult EliminarAfiliado(string correo)
        {
            sistem14_ropa_mexicanaContext Context = new sistem14_ropa_mexicanaContext();
            MarcaRepository repos = new MarcaRepository(Context);
            var afiliado = repos.GetUsByCorreo(correo);

            if (afiliado != null)
            {
                HttpContext.SignOutAsync();
                repos.Delete(afiliado);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "El usuario no se ha podido eliminar.");
                return RedirectToAction("SesionIniciada");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}

