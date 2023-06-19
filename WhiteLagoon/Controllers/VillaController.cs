using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IWebHostEnvironment webHostEnvironment, IVillaService villaService)
        {
            _webHostEnvironment = webHostEnvironment;
            _villaService = villaService;
        }
        public IActionResult Index()
        {
            List<Villa> villaList = _villaService.GetAll();
            return View(villaList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description?.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
                TempData["error"] = "Error encountered";
            }
            if (ModelState.IsValid)
            {
                _villaService.Create(villa, _webHostEnvironment.WebRootPath);

                TempData["success"] = "Villa Created Successfully";
                return RedirectToAction("Index");
            }
            return View(villa);

        }

        public IActionResult Update(int villaId)
        {
            Villa? villa = _villaService.GetById(villaId);

            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid && villa.Id > 0)
            {
                _villaService.Update(villa, _webHostEnvironment.WebRootPath);

                TempData["success"] = "Villa Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(villa);

        }

        public IActionResult Delete(int villaId)
        {
            Villa? villa = _villaService.GetById(villaId);

            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);

        }
        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? villa = _villaService.GetById(obj.Id);
            if (villa != null)
            {
                TempData["success"] = "Villa Deleted Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var villas = _villaService.GetAll();
            return Json(new { data = villas });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var villaToBeDeleted = _villaService.GetById(id ?? 0);
            if (villaToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _villaService.Delete(villaToBeDeleted, _webHostEnvironment.WebRootPath);

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
