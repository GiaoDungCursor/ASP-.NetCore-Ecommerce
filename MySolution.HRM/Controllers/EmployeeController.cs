using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySolution.BusinessLayers;
using MySolution.DomainModels;
using System.Collections.Generic;

namespace MySolution.HRM.Controllers
{
    public class EmployeeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var model = await HRMService.ListEmployeeAsync();
            return View(model);
           
        }
        [HttpGet]
        public IActionResult Create()
        {
            var model = new Employee();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Employee model)
        {
            await HRMService.AddEmployeeAsync(model);
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Delete(string id)
        {
            await HRMService.DeleteEmployeeAsync(id);
            return RedirectToAction("Index");
        }
    }
}
