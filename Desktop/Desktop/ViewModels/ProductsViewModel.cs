using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Data;
using Desktop.Models;

namespace Desktop.ViewModels;

public partial class ProductsViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<Product> _products = new();

    [ObservableProperty] private Product? _selectedProduct;

    [ObservableProperty] private ObservableCollection<Category> _categories = new();

    public ProductsViewModel() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        var db = DatabaseService.Instance;
        var products = await db.GetProductsAsync();
        Products = new ObservableCollection<Product>(products);
        Categories = new ObservableCollection<Category>(await db.GetCategoriesAsync());
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (Categories.Count == 0)
            return;

        var product = new Product
        {
            Name = "",
            CurrentPrice = 0,
            StockQuantity = 0,
            CategoryId = Categories.First().CategoryId
        };
        await DatabaseService.Instance.AddProductAsync(product);
        Products.Add(product);
        SelectedProduct = product;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedProduct is null) return;
        await DatabaseService.Instance.DeleteProductAsync(SelectedProduct.ProductId);
        Products.Remove(SelectedProduct);
        SelectedProduct = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedProduct is null) return;
        await DatabaseService.Instance.UpdateProductAsync(SelectedProduct);
        var cat = Categories.FirstOrDefault(c => c.CategoryId == SelectedProduct.CategoryId);
        if (cat != null) SelectedProduct.CategoryName = cat.Name;
        var index = Products.IndexOf(SelectedProduct);
        if (index >= 0) Products[index] = SelectedProduct;
    }
}