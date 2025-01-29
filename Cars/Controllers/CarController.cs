using Cars.AppDbContext;
using Cars.Models;
using Cars.Models.ViewModels;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace Cars.Controllers
{
    public class CarController : Controller
    {
        private readonly VroomDbContext vroomDbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public CarViewModel CarVM { get; set; }

        public CarController(VroomDbContext vroomDbContext, IWebHostEnvironment hostingEnvironment)
        {
            this.vroomDbContext = vroomDbContext;
            CarVM = new CarViewModel()
            {
                Makes = vroomDbContext.Makes.ToList(),
                Models = vroomDbContext.Models.ToList(),
                Car = new Car()
            };
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 2)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "Price_desc" : "";

            int excludeRecords = (pageSize * pageNumber) - pageSize;

            var cars = vroomDbContext.Cars.Include(m => m.Make).Include(m => m.Model).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(b => b.Make.Name.Contains(searchString));
            }

            cars = sortOrder == "Price_desc"
                ? cars.OrderByDescending(b => b.Price)
                : cars.OrderBy(b => b.Price);

            var pagedResult = new PagedResult<Car>
            {
                Data = cars.Skip(excludeRecords).Take(pageSize).AsNoTracking().ToList(),
                TotalItems = cars.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewBag.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalItems / pageSize);

            return View(pagedResult);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(CarVM);
        }

        [HttpPost, ActionName("Create")]
        public IActionResult CreatePost()
        {
            vroomDbContext.Add(this.CarVM.Car);
            vroomDbContext.SaveChanges();
            // var carId = CarVM.Car.Id;

            // string wwwrootPath = _hostingEnvironment.WebRootPath;
            // var files = HttpContext.Request.Form.Files;
            // var savedCar = vroomDbContext.Cars.Find(carId);

            // if (files.Count != 0)
            // {
            //     var imagePath = @"images\Car\";
            //     var extension = Path.GetExtension(files[0].FileName);
            //     var relativeImagePath = imagePath + carId + extension;
            //     var absoluteImagePath = Path.Combine(wwwrootPath, relativeImagePath);

            //     using (var fileStream = new FileStream(absoluteImagePath, FileMode.Create))
            //     {
            //         files[0].CopyTo(fileStream);
            //     }
            //     savedCar.ImagePath = relativeImagePath;
            //     vroomDbContext.SaveChanges();
            // }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Car/Delete/{id}")]

        public IActionResult Delete(int id)
        {
            var car = vroomDbContext.Cars.Find(id);
            if (car == null)
            {
                return NotFound();
            }
            vroomDbContext.Cars.Remove(car);
            vroomDbContext.SaveChanges();
           
            return RedirectToAction(nameof(Index));
        }
        // [HttpGet("Car/Edit/{id}")]
        // public IActionResult Edit(int id)
        // {
        //     return View(CarVM);
        // }

        [HttpGet("Car/Edit/{id}")]
public IActionResult Edit(int id)
{
    // Find the car by its ID
    CarVM.Car = vroomDbContext.Cars.Find(id);
    if (CarVM.Car == null)
    {
        return NotFound();
    }
    // Populate the ViewModel with existing makes and models for dropdowns
    CarVM.Makes = vroomDbContext.Makes.ToList();
    CarVM.Models = vroomDbContext.Models.ToList();

    return View(CarVM);
}


 [HttpPost, ActionName("Edit")]
        public IActionResult EditPost()
        {
           vroomDbContext.Update(this.CarVM.Car);
           vroomDbContext.SaveChanges();
           return RedirectToAction(nameof(Index));
        
        }




    }
}
