using Cars.AppDbContext;
using Cars.Helpers;
using Cars.Models;
using Cars.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using cloudscribe.Pagination.Models;
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
                Car = new Models.Car(),
            };
            _hostingEnvironment = hostingEnvironment;

        }


        public IActionResult Index(String searchString, String sortOrder,int pageNumber=1, int pageSize=2)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = String.IsNullOrEmpty(sortOrder) ? "Price_desc" : "";
            int ExcludeRecords = (pageSize * pageNumber) - pageSize;
            var Cars = from b in vroomDbContext.Cars.Include(m => m.Make).Include(m => m.Model)
                        select b;
            var CarCount = Cars.Count();
            if (!String.IsNullOrEmpty(searchString) )
            {
                Cars = Cars.Where(b => b.Make.Name.Contains(searchString));
                CarCount = Cars.Count();
            }
            //Sorting Logic
            switch (sortOrder)
            {
                case "Price_desc":
                    Cars = Cars.OrderByDescending(b => b.Price);
                    break;
                default:
                    Cars = Cars.OrderBy(b => b.Price);
                    break;
            }
            Cars = Cars
                .Skip(ExcludeRecords)
                .Take(pageSize);
            var result = new PagedResult<Car>
            {
                Data = Cars.AsNoTracking().ToList(),
                TotalItems = CarCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(result);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            CarVM.Car= vroomDbContext.Cars.SingleOrDefault(b=> b.Id == id);
            CarVM.Models= vroomDbContext.Models.Where(m=>m.Make== CarVM.Car.Make);
            if (CarVM.Car == null )
            {
                return NotFound();
            }
            return View(CarVM);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(CarVM);
        }

        [HttpPost, ActionName("Create")]
        public IActionResult CreatePost()
        {
            //if (ModelState.IsValid)
            //{
            vroomDbContext.Add(this.CarVM.Car);
            vroomDbContext.SaveChanges();
            var CarId = CarVM.Car.Id;
            string wwrootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var savedCar = vroomDbContext.Cars.Find(CarId);
            if (files.Count != 0)
            {
                var ImagePath = @"images\Car\";
                var extension = Path.GetExtension(files[0].FileName);
                var relativeImagePath = ImagePath + CarId + extension;
                var absImagePath = Path.Combine(wwrootPath, relativeImagePath);

                using (var FileStream = new FileStream(absImagePath, FileMode.Create))
                {
                    files[0].CopyTo(FileStream);
                }
                savedCar.ImagePath = relativeImagePath;
                vroomDbContext.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
            //}
            //return View(modelVm);
        }


        public IActionResult Delete(int id)
        {
            {
                var Car = vroomDbContext.Cars.Find(id);
                if (Car == null)
                {
                    return NotFound();
                }
                vroomDbContext.Cars.Remove(Car);
                vroomDbContext.SaveChanges();
                return (RedirectToAction(nameof(Index)));
            }
        }


        //[HttpGet]
        //public IActionResult Edit(int id)
        //{
        //    CarVM.Car = vroomDbContext.Cars.Include(m => m.Make).Include(m => m.Model).SingleOrDefault(m => m.Id == id);
        //    if (CarVM.Car == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(CarVM);
        //}


        //[HttpPost, ActionName("Edit")]
        //public IActionResult EditPost()
        //{
        //    vroomDbContext.Update(this.CarVM.Car);
        //    vroomDbContext.SaveChanges();
        //    return (RedirectToAction(nameof(Index)));
        //
        //}
    }
}
