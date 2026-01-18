# MyPOS99 - Point of Sale System

A modern WPF-based Point of Sale system built with .NET 10 and SQLite.

## ?? Quick Start

1. **Clone and open** the project in Visual Studio 2022 or later
2. **Build** the solution (Ctrl+Shift+B)
3. **Run** the application (F5)
4. Default login: Username: `admin`, Password: `admin123`

## ?? Project Structure

```
MyPOS99/
??? Models/              # Data models and entities
?   ??? User.cs
?   ??? Product.cs
?   ??? Customer.cs
?   ??? Supplier.cs
?   ??? Sale.cs
?   ??? SaleItem.cs
?   ??? Purchase.cs
?   ??? PurchaseItem.cs
?   ??? Expense.cs
?   ??? Category.cs
?
??? Views/               # WPF views (XAML)
?   ??? MainWindow.xaml
?   ??? MainWindow.xaml.cs
?
??? ViewModels/          # MVVM view models
?   ??? ViewModelBase.cs       # Base class with INotifyPropertyChanged
?   ??? MainViewModel.cs
?   ??? RelayCommand.cs        # ICommand implementation
?
??? Services/            # Business logic layer
?   ??? ProductService.cs
?   ??? SaleService.cs
?   ??? UserService.cs
?   ??? ExpenseService.cs
?   ??? CategoryService.cs
?   ??? CustomerService.cs
?   ??? SupplierService.cs
?
??? Data/                # Database layer
?   ??? DatabaseContext.cs          # DatabaseService + connection
?   ??? DATABASE_SCHEMA.md          # Schema documentation
?   ??? DATABASE_SERVICE_GUIDE.md   # Usage guide
?
??? App.xaml            # Application resources and startup
```

## ??? Architecture

This project follows the **MVVM (Model-View-ViewModel)** pattern:

- **Models**: Data entities representing database tables
- **Views**: XAML UI components
- **ViewModels**: Presentation logic with data binding
- **Services**: Business logic and data access
- **Data**: Database operations with parameterized queries

## ?? Database

### Tables
The application uses **SQLite** with the following schema:

| Table | Description |
|-------|-------------|
| **Users** | System users (Admin, Cashier, Manager) |
| **Products** | Product inventory with pricing & stock |
| **Customers** | Customer database with purchase history |
| **Suppliers** | Vendor/supplier information |
| **Sales** | Sales transactions |
| **SaleItems** | Sale line items |
| **Purchases** | Purchase orders from suppliers |
| **PurchaseItems** | Purchase line items |
| **Expenses** | Business expense tracking |
| **Categories** | Product categories |

### DatabaseService Features
- ? Parameterized queries (SQL injection protection)
- ? Transaction support for atomic operations
- ? Async/await for all operations
- ? Generic query execution with mapping
- ? Automatic connection management

**See:** `Data/DATABASE_SERVICE_GUIDE.md` for detailed usage

## ??? Technologies

- **.NET 10** - Latest .NET framework
- **WPF** - Windows Presentation Foundation
- **SQLite** - Embedded database (Microsoft.Data.Sqlite)
- **MVVM Pattern** - Model-View-ViewModel architecture
- **C# 12** - Latest language features

## ?? Screenshots

### Login Screen
Secure authentication with role-based access control.

![Login Screen](Screenshots/Login.png)

### Dashboard
Comprehensive overview with key metrics and quick actions.

![Dashboard](Screenshots/Dashboard.png)

### Sales/POS Interface
Intuitive point-of-sale system with cart management and multiple payment options.

![Sales Screen](Screenshots/Sales.png)

### Product Management
Complete inventory management with stock tracking and category organization.

![Product Management](Screenshots/Product.png)

### Customer Management
Customer database with purchase history and contact information.

![Customer Management](Screenshots/Customer.png)

### Supplier Management
Supplier tracking and relationship management.

![Supplier Management](Screenshots/Supplier.png)

### Expense Tracking
Business expense monitoring and categorization.

![Expense Tracking](Screenshots/Expense.png)

### Reports & Analytics
Comprehensive reporting system with multiple report types.

![Reports Dashboard](Screenshots/Reports.png)

### PDF Report Generation
Professional PDF exports for all report types.

![PDF Report Sample](Screenshots/Report%20PDf.png)

## ?? Documentation

- **Database Schema**: See `Data/DATABASE_SCHEMA.md`
- **Service Usage**: See `Data/DATABASE_SERVICE_GUIDE.md`
- **API Reference**: See inline code comments

## ? Key Features

### Core Features (Fully Implemented)
- ? **User Authentication & Authorization** - Role-based access control (Admin, Cashier, Manager)
- ? **Dashboard** - Real-time metrics and quick action buttons
- ? **Sales/POS Interface** - Complete point-of-sale system with cart management
- ? **Product Management** - Full CRUD operations with stock tracking
- ? **Inventory Management** - Automatic stock updates and low-stock alerts
- ? **Customer Management** - Customer database with purchase history
- ? **Supplier Management** - Supplier tracking and contact management
- ? **Purchase Orders** - Purchase entry and inventory receiving
- ? **Expense Tracking** - Business expense monitoring and categorization
- ? **Reports & Analytics** - Comprehensive reporting with PDF/Excel export
- ? **Return/Exchange Management** - Sales return and exchange processing
- ? **Multi-Payment Support** - Cash, Card, Mobile, and Credit payment types
- ? **Receipt Generation** - Automated PDF receipt creation

### Technical Implementation
- ? MVVM architecture with ViewModelBase
- ? SQLite database with 10 normalized tables
- ? Secure parameterized queries (SQL injection protection)
- ? Complete service layer (10+ services)
- ? Transaction support for data integrity
- ? Async/await patterns throughout
- ? PDF and Excel export functionality
- ? Custom WPF value converters
- ? Comprehensive data validation

## ?? Security

- Password hashing placeholder (implement BCrypt recommended)
- Parameterized SQL queries (SQL injection protection)
- Role-based access control structure (Admin, Cashier, Manager)

## ?? Usage Example

```csharp
// Initialize services
var dbService = new DatabaseService();
var productService = new ProductService(dbService);

// Add a product
var product = new Product
{
    Code = "P001",
    Name = "Laptop",
    CostPrice = 35000,
    SellPrice = 45000,
    StockQty = 10,
    MinStockLevel = 2
};
await productService.AddProductAsync(product);

// Get all products
var products = await productService.GetAllProductsAsync();

// Search products
var searchResults = await productService.SearchProductsAsync("Laptop");
```

## ??? Development Roadmap

### Phase 1 - UI Development (Next)
- [ ] Login screen
- [ ] Dashboard
- [ ] Product management screen
- [ ] POS/Sales screen

### Phase 2 - Features
- [ ] Barcode scanning
- [ ] Receipt printing
- [ ] Inventory alerts
- [ ] Reports

### Phase 3 - Advanced
- [ ] Multi-user support
- [ ] Data export
- [ ] Backup/restore
- [ ] Cloud sync (optional)

## ?? Contributing

This is a learning/demonstration project. Feel free to:
- Add new features
- Improve the UI
- Add unit tests
- Enhance documentation

## ?? License

This project is for educational purposes.

## ?? Support

For issues or questions:
1. Check `Data/DATABASE_SERVICE_GUIDE.md` for database operations
2. Review `Data/DATABASE_SCHEMA.md` for schema details
3. See inline code comments for specific implementations

---

**Built with ?? using .NET 10 and WPF**
