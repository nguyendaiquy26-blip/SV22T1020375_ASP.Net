using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Catalog;
using SV22T1020375.Models.Common;

namespace SV22T1020375.Admin.Controllers
{
    [Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
    public class ProductController : Controller
    {
        private async Task PrepareSelectListsAsync()
        {
            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = ""
            });

            var suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = ""
            });

            ViewBag.Categories = new SelectList(categories.DataItems, "CategoryID", "CategoryName");
            ViewBag.Suppliers = new SelectList(suppliers.DataItems, "SupplierID", "SupplierName");
        }

        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>("ProductSearchInput");
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            ViewBag.Title = "Quản lý Mặt hàng";
            await PrepareSelectListsAsync();
            return View(input);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            if (input.Page < 1) input.Page = 1;
            if (input.PageSize <= 0) input.PageSize = ApplicationContext.PageSize;
            input.SearchValue ??= "";

            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData("ProductSearchInput", input);

            return PartialView(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            ViewBag.Title = "Chi tiết sản phẩm";
            var data = await CatalogDataService.GetProductAsync(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            await PrepareSelectListsAsync();

            var model = new Product()
            {
                ProductID = 0,
                Price = 0,
                IsSelling = true,
                Photo = ""
            };

            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật mặt hàng";
            await PrepareSelectListsAsync();

            var data = await CatalogDataService.GetProductAsync(id);
            if (data == null) return RedirectToAction("Index");

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto)
        {
            ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
            await PrepareSelectListsAsync();

            if (string.IsNullOrWhiteSpace(data.ProductName))
                ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");
            if (!data.CategoryID.HasValue || data.CategoryID <= 0)
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
            if (!data.SupplierID.HasValue || data.SupplierID <= 0)
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
            if (string.IsNullOrWhiteSpace(data.Unit))
                ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị tính");
            if (data.Price < 0)
                ModelState.AddModelError(nameof(data.Price), "Giá bán không được âm");
            if (string.IsNullOrEmpty(data.ProductDescription))
                data.ProductDescription = "";

            if (!ModelState.IsValid) return View("Edit", data);

            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                Directory.CreateDirectory(folder);
                string fileName = $"{DateTime.Now.Ticks}{Path.GetExtension(uploadPhoto.FileName)}";
                string filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            if (data.ProductID == 0)
            {
                await CatalogDataService.AddProductAsync(data);
                TempData["Message"] = "Thêm mặt hàng thành công!";
            }
            else
            {
                await CatalogDataService.UpdateProductAsync(data);
                TempData["Message"] = "Cập nhật mặt hàng thành công!";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            // 1. Nếu là Request POST (Người dùng bấm nút "Xác nhận xóa" ở trang Delete.cshtml)
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteProductAsync(id);
                TempData["Message"] = "Đã xóa mặt hàng thành công!";
                return RedirectToAction("Index"); // Quay về trang danh sách
            }

            // 2. Nếu là Request GET (Người dùng bấm nút Xóa ở ngoài danh sách)
            ViewBag.Title = "Xác nhận xóa mặt hàng";
            var data = await CatalogDataService.GetProductAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            // Kiểm tra xem có dữ liệu ràng buộc không
            ViewBag.AllowDelete = !(await CatalogDataService.IsUsedProductAsync(id));

            return View(data); // Hiển thị trang Delete.cshtml
        }

        // ================= XỬ LÝ THUỘC TÍNH (ATTRIBUTES) =================

        public async Task<IActionResult> ListAttribute(int id)
        {
            ViewBag.ProductID = id;
            var data = await CatalogDataService.ListAttributesAsync(id);
            return PartialView(data);
        }

        public IActionResult CreateAttribute(int id)
        {
            ViewBag.Title = "Bổ sung thuộc tính";
            ViewBag.ProductID = id;
            var model = new ProductAttribute() { ProductID = id, AttributeID = 0 };
            return View("EditAttribute", model);
        }

        public async Task<IActionResult> EditAttribute(int id, long attribute)
        {
            ViewBag.Title = "Cập nhật thuộc tính";
            ViewBag.ProductID = id;
            var data = await CatalogDataService.GetAttributeAsync(attribute);
            if (data == null) return RedirectToAction("Edit", new { id = id });
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Vui lòng nhập tên thuộc tính");
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Vui lòng nhập giá trị");
            if (data.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.AttributeID == 0 ? "Bổ sung thuộc tính" : "Cập nhật thuộc tính";
                ViewBag.ProductID = data.ProductID;
                return View("EditAttribute", data);
            }

            if (data.AttributeID == 0)
            {
                await CatalogDataService.AddAttributeAsync(data);
                TempData["Message"] = "Đã thêm thuộc tính mới!";
            }
            else
            {
                await CatalogDataService.UpdateAttributeAsync(data);
                TempData["Message"] = "Cập nhật thuộc tính thành công!";
            }

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        public async Task<IActionResult> DeleteAttribute(int id, long attribute)
        {
            await CatalogDataService.DeleteAttributeAsync(attribute);
            TempData["Message"] = "Đã xóa thuộc tính!";
            return RedirectToAction("Edit", new { id });
        }

        // ================= XỬ LÝ HÌNH ẢNH (PHOTOS) =================

        public async Task<IActionResult> ListPhoto(int id)
        {
            ViewBag.ProductID = id;
            var data = await CatalogDataService.ListPhotosAsync(id);
            return PartialView(data);
        }

        public IActionResult CreatePhoto(int id)
        {
            ViewBag.Title = "Bổ sung ảnh";
            ViewBag.ProductID = id;
            var model = new ProductPhoto() { ProductID = id, PhotoID = 0, IsHidden = false };
            return View("EditPhoto", model);
        }

        public async Task<IActionResult> EditPhoto(int id, long photoId)
        {
            ViewBag.Title = "Cập nhật ảnh";
            ViewBag.ProductID = id;
            var data = await CatalogDataService.GetPhotoAsync(photoId);
            if (data == null) return RedirectToAction("Edit", new { id = id });
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            if (string.IsNullOrWhiteSpace(data.Description)) data.Description = "";
            if (data.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.PhotoID == 0 ? "Bổ sung ảnh" : "Cập nhật ảnh";
                ViewBag.ProductID = data.ProductID;
                return View("EditPhoto", data);
            }

            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                Directory.CreateDirectory(folder);
                string fileName = $"{DateTime.Now.Ticks}{Path.GetExtension(uploadPhoto.FileName)}";
                string filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            if (data.PhotoID == 0)
            {
                await CatalogDataService.AddPhotoAsync(data);
                TempData["Message"] = "Đã thêm hình ảnh mới!";
            }
            else
            {
                await CatalogDataService.UpdatePhotoAsync(data);
                TempData["Message"] = "Cập nhật hình ảnh thành công!";
            }

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        public async Task<IActionResult> DeletePhoto(int id, long photoId)
        {
            await CatalogDataService.DeletePhotoAsync(photoId);
            TempData["Message"] = "Đã xóa hình ảnh!";
            return RedirectToAction("Edit", new { id });
        }
    }
}