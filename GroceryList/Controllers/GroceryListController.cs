using GroceryList.Data.Caching;
using GroceryList.Data.UnitOfWork;
using GroceryList.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GroceryList.Controllers
{
  [ApiController]
  [Route("api/")]
  public class GroceryListController : ControllerBase
  {
    IUnitOfWork _unitOfWork;
    ILogger<GroceryListController> _logger;

		public GroceryListController(IUnitOfWork unitOfWork, ILogger<GroceryListController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("IsUp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult IsUp()
    {
      _logger.LogTrace("IsUp");
      return Ok();
    } 

		[HttpGet]
		[Authorize]
		[Route("GetCategory")]
		[ProducesResponseType(typeof(CategoryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetCategory(string id)
		{
			_logger.LogTrace("GetCategory");
			try
			{
				CategoryModel? c = await _unitOfWork.GroceryListRepository().GetCategory(id);

        if(c == null) return NotFound("Category not found!");

				return Ok(c);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet]
		[Authorize]
		[Route("GetCategoryList")]
		[ProducesResponseType(typeof(List<CategoryModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
		public async Task<IActionResult> GetCategoryList()
		{
			_logger.LogTrace("GetCategoryList");
			try
			{
				List<CategoryModel>? c = await _unitOfWork.GroceryListRepository().GetCategoryList();

        if(c == null) return StatusCode(503, "The server is currently unable to access the database.");

				return Ok(c);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet]
		[Authorize]
		[Route("GetItemListInCategory")]
		[ProducesResponseType(typeof(List<ItemModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
		public async Task<IActionResult> GetItemListInCategory(string categoryId)
		{
			_logger.LogTrace("GetItemListInCategory");
			try
			{
				List<ItemModel>? c = await _unitOfWork.GroceryListRepository().GetItemListInCategory(categoryId);

        if(c == null)
          return StatusCode(503, "The server is currently unable to access the database.");

				return Ok(c);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

    [HttpPut]
    [Authorize]
    [Route("PutCategory")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutCategory(CategoryModel c)
    {
			_logger.LogTrace("PutCategory");

			try
			{
				CategoryModel? category = await _unitOfWork.GroceryListRepository().DoesCategoryAlreadyExist(c);
          
        if(category == null)
        {
          CategoryModel? newCategory = await _unitOfWork.GroceryListRepository().PutCategory(c);

          if(newCategory != null)
            return Ok(newCategory);
          else
          {
            _logger.LogError("Error adding category to database.");
            return StatusCode(500, "Error adding category to database.");
          }
        }
        else
        {
          return Conflict("Category already exist!");
        }
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPut]
		[Authorize]
		[Route("PutItem")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> PutItem(ItemModel i)
		{
			_logger.LogTrace("PutItem");

			try
			{
				ItemModel? item = await _unitOfWork.GroceryListRepository().DoesItemWithSameNameAlreadyExist(i);
        if(item == null)
        {
          ItemModel? newItem = await _unitOfWork.GroceryListRepository().PutItem(i);
          if(newItem != null)
            return Ok(newItem);
          else
          {
            _logger.LogError("Error adding item to database.");
				    return StatusCode(500, "Error adding item to database.");
          }
        }
        else
        {
          return Conflict("Item already exist!");
        }
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPatch]
		[Authorize]
		[Route("PatchCategory")]
		[ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> PatchCategory(CategoryModel c)
		{
			_logger.LogTrace("PatchCategory");

			try
			{
        CategoryModel? category = await _unitOfWork.GroceryListRepository().DoesCategoryAlreadyExist(c);

				if(category == null)
        {
          category = await _unitOfWork.GroceryListRepository().PatchCategory(c);
          if(category != null)
            return Ok(category);
          else
            return NotFound("Category not found!");
        }
        else
        {
          return Conflict("Category already exist!");
        }
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPatch]
		[Authorize]
		[Route("PatchItem")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> PatchItem(ItemModel i)
		{
			_logger.LogTrace("PatchItem");

			try
			{
        ItemModel? item = await _unitOfWork.GroceryListRepository().DoesItemWithSameNameAlreadyExist(i);

				if(item == null)
        {
          ItemModel? responseItem = await _unitOfWork.GroceryListRepository().PatchItem(i);
          if(responseItem != null){
            return Ok(responseItem);
          }
          else{
            return NotFound("Server can't find this item on database.");
          }
        }
        else
        {
          return Conflict("Item already exist!");
        }
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpDelete]
		[Authorize]
		[Route("DeleteCategory")]
		[ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteCategory(CategoryModel c)
		{
			_logger.LogTrace("DeleteCategory");

			try
			{
        if(c.id == null) return BadRequest("Category id missing!");

        CategoryModel? existingCategory = await _unitOfWork.GroceryListRepository().GetCategory(c.id);

        if(existingCategory == null) return NotFound("Category not found!");
        
				bool result = await _unitOfWork.GroceryListRepository().DeleteCategory(existingCategory);

        if(result)
          return Ok();
        else
          return StatusCode(500, "Something went wrong deleting the category!");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

		[HttpDelete]
		[Authorize]
		[Route("DeleteItem")]
		[ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteItem(ItemModel i)
		{
			_logger.LogTrace("DeleteItem");

			try
			{
        if(i.id == null) return BadRequest("Item id is missing!");

        ItemModel? existingItem = await _unitOfWork.GroceryListRepository().GetItem(i.id);

        if(existingItem == null) return NotFound("Item not found!");

				bool result = await _unitOfWork.GroceryListRepository().DeleteItem(i);

			  if(result) return Ok();
        else return StatusCode(500, "Something went wrong deleting the item!");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
		}

    [HttpGet]
		[Authorize]
		[Route("GetGroceryList")]
		[ProducesResponseType(typeof(GroceryListModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroceryList()
    {
      _logger.LogTrace("GetGroceryList");

			try
			{
				GroceryListModel result = await _unitOfWork.GroceryListRepository().GetGroceryList();

			  return Ok(result);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
    }

    [HttpPut]
		[Authorize]
		[Route("SyncGroceryList")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncGroceryList(GroceryListModel model)
    {
      _logger.LogTrace("SaveGroceryList");

			try
			{
				GroceryListModel? result = await _unitOfWork.GroceryListRepository().SyncGroceryList(model);

			  if(result != null) return Ok(result);
        else return StatusCode(500, "Something went wrong saving grocery list!");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
			}
    }

		// [HttpGet]
		// [Authorize]
		// [Route("ChangeDisplayAllCategories")]
		// [ProducesResponseType(StatusCodes.Status200OK)]
		// [ProducesResponseType(StatusCodes.Status500InternalServerError)]
		// public IActionResult ChangeDisplayAllCategories(bool value)
		// {
		// 	_logger.LogTrace("ChangeDisplayAllCategories");
		// 	try
		// 	{
		// 		_unitOfWork.GroceryListRepository().ChangeDisplayAllCategories(value);
		// 		return Ok();
		// 	}
		// 	catch(Exception ex)
		// 	{
		// 		_logger.LogError(ex.Message);
		// 		return StatusCode(500, ex.Message);
		// 	}
		// }
	}
}
