using System.Data;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebAPIWithEF.Models;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis;

namespace WebAPIWithEF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly StoreContext _context;
        private string serializedObject = "";

        public ProductsController(StoreContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        [Route("ProductList")]
        public async Task<JsonResult> GetProductList()
        {
            List<ProductDataClient> productList = await (from p in _context.Products
                                                         join t in _context.ProductTypes
                                                           on p.ProductTypeId equals t.ProductTypeId
                                                         orderby p.ProductTypeId, p.Name
                                                         select new ProductDataClient
                                                         {
                                                             id = p.ProductId,
                                                             name = p.Name,
                                                             price = p.Price,
                                                             typeId = p.ProductTypeId,
                                                             typeName = t.TypeName
                                                         }).ToListAsync();

            if (productList.Count > 0)
            {
                serializedObject = JsonConvert.SerializeObject(productList);
                return new JsonResult(serializedObject);
            }
            else
            {
                serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "No data found." });
                return new JsonResult(serializedObject);
            }
        }

        // GET: Product Types
        [HttpGet]
        [Route("ProductTypes")]
        public async Task<JsonResult> GetProductTypes()
        {
            List<ProductType> productTypes = await (from t in _context.ProductTypes
                                                    select new ProductType
                                                    {
                                                        ProductTypeId = t.ProductTypeId,
                                                        TypeName = t.TypeName
                                                    }).ToListAsync();

            if (productTypes.Count > 0)
            {
                serializedObject = JsonConvert.SerializeObject(productTypes);
                return new JsonResult(serializedObject);
            }
            else
            {
                serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "No data found." });
                return new JsonResult(serializedObject);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetProduct()
        {
            {
                var parsed = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
                if (parsed["productId"] == null)
                {
                    serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "The key 'productId' does not exist." });
                    return new JsonResult(serializedObject);
                }
                int productId = Convert.ToInt32(parsed["productId"]);

                Product? product = await _context.Products.FirstOrDefaultAsync(el => el.ProductId == productId);
                ProductClient productClient = new ProductClient();

                if (product != null)
                {
                    productClient.id = product.ProductId;
                    productClient.name = product.Name;
                    productClient.price = product.Price;
                    productClient.typeId = product.ProductTypeId;
                    productClient.typeName = "";
                    serializedObject = JsonConvert.SerializeObject(productClient);
                    return new JsonResult(serializedObject);
                }
                else
                {
                    serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "No data found." });
                    return new JsonResult(serializedObject);
                }
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddProduct()
        {
            string? productStringified = HttpContext.Request.Headers["product"];
            if (productStringified == null)
            {
                return new JsonResult("Error: Cannot insert the record: " +
                  "Http Header's object is null.");
            }
            JToken token = JObject.Parse(productStringified);
            Product product = new Product();
            product = SetProduct(product, token);
            ProductDataClient productDataClient = new ProductDataClient
            {
                id = product.ProductId,
                name = product.Name,
                price = product.Price,
                typeId = product.ProductTypeId,
                typeName = ""
            };
            try
            {
                await _context.Products.AddAsync(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                string? message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                productDataClient.name = "Error: Cannot insert the record with ID=" + product.ProductId +
                    ". " + message + ".";
                Console.WriteLine(message);
            }

            return new JsonResult(productDataClient);
        }

        [HttpPost]
        [Route("UpdateProduct")]

        public async Task<JsonResult> UpdateProduct()
        {
            string? productStringified = HttpContext.Request.Headers["product"];

            if (productStringified == null)
            {
                return new JsonResult("Error: Cannot update the record: " +
                  "Http Header's object is null.");
            }
            string message = "";
            try
            {
                JToken token = JObject.Parse(productStringified);
                Product updatedProduct = new Product();
                updatedProduct = SetProduct(updatedProduct, token);
                message = "Product with id= " + updatedProduct.ProductId + " is Updated!";

                Product? product = await _context.Products.FirstOrDefaultAsync(el => el.ProductId == updatedProduct.ProductId);

                if (product != null)
                {

                    product.ProductId = updatedProduct.ProductId;
                    product.Name = updatedProduct.Name;
                    product.Price = updatedProduct.Price;
                    product.ProductTypeId = updatedProduct.ProductTypeId;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    message = "Cannot update the record with id= " + updatedProduct.ProductId + ". The specified record does not exist!";
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }

            return new JsonResult(message);
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteProduct()
        {
            var parsed = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
            int productId = Convert.ToInt32(parsed["productId"]);

            var result = new Result();
            result.id = productId;

            try
            {
                int rowsDeleted = await _context.Products.Where(t => t.ProductId == productId).ExecuteDeleteAsync();
                if (rowsDeleted == 0)
                {
                    result.error = "Error: Cannot delete the record with ID= " +
                        productId + ". The specified record does not exist.";
                    return new JsonResult(result.error);
                }
            }

            catch (Exception ex)
            {
                string? message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                result.error = "Error: Cannot delete the record with ID=" +
                    productId + ". " + message + ".";
                return new JsonResult(result.error);
            }
            result.message = "The record with productID=" + productId + " deleted!";
            return new JsonResult(result.message);
        }

        private Product SetProduct(Product product, JToken token)
        {
            product.ProductId = Convert.ToInt32(token.SelectToken("id"));
            product.Name = Convert.ToString(token.SelectToken("name"));
            product.Price = Convert.ToDecimal(token.SelectToken("price"));
            product.ProductTypeId = Convert.ToInt32(token.SelectToken("typeId"));
            return product;
        }
    }
}
