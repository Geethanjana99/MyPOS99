# Database Schema Documentation

## Overview
This document describes the database schema for the MyPOS99 Point of Sale system.

## Database Tables

### 1. Users
Stores system users with authentication and role information.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Username | TEXT | Unique username |
| PasswordHash | TEXT | Hashed password |
| Role | TEXT | User role (Admin, Cashier, Manager) |
| CreatedAt | TEXT | Account creation timestamp |
| IsActive | INTEGER | Active status (1=active, 0=inactive) |

### 2. Products
Core product inventory management.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Code | TEXT | Unique product code |
| Name | TEXT | Product name |
| Category | TEXT | Product category |
| CostPrice | REAL | Purchase/cost price |
| SellPrice | REAL | Selling price |
| StockQty | INTEGER | Current stock quantity |
| MinStockLevel | INTEGER | Minimum stock alert level |
| Barcode | TEXT | Product barcode |
| CreatedAt | TEXT | Creation timestamp |
| UpdatedAt | TEXT | Last update timestamp |

### 3. Suppliers
Supplier/vendor information.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Supplier name |
| Phone | TEXT | Contact phone |
| Email | TEXT | Email address |
| Address | TEXT | Physical address |
| CreatedAt | TEXT | Creation timestamp |
| IsActive | INTEGER | Active status |

### 4. Customers
Customer information and purchase history.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Customer name |
| Phone | TEXT | Contact phone |
| Email | TEXT | Email address |
| Address | TEXT | Physical address |
| TotalPurchases | REAL | Total purchase amount |
| CreatedAt | TEXT | Registration timestamp |
| IsActive | INTEGER | Active status |

### 5. Sales
Sales transaction header information.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| InvoiceNumber | TEXT | Unique invoice number |
| Date | TEXT | Sale date |
| SubTotal | REAL | Subtotal before discount/tax |
| Discount | REAL | Total discount amount |
| Tax | REAL | Tax amount |
| Total | REAL | Final total amount |
| PaymentType | TEXT | Payment method (Cash, Card, Mobile, Credit) |
| AmountPaid | REAL | Amount paid by customer |
| Change | REAL | Change given |
| UserId | INTEGER | Foreign key to Users |
| CustomerId | INTEGER | Foreign key to Customers (optional) |
| Notes | TEXT | Additional notes |
| CreatedAt | TEXT | Transaction timestamp |

### 6. SaleItems
Individual items in a sale transaction.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| SaleId | INTEGER | Foreign key to Sales |
| ProductId | INTEGER | Foreign key to Products |
| ProductCode | TEXT | Product code (denormalized) |
| ProductName | TEXT | Product name (denormalized) |
| Qty | INTEGER | Quantity sold |
| Price | REAL | Unit price at time of sale |
| Discount | REAL | Discount on this item |
| Total | REAL | Line total |

### 7. Purchases
Purchase order header information.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| PurchaseNumber | TEXT | Unique purchase order number |
| SupplierId | INTEGER | Foreign key to Suppliers |
| Date | TEXT | Purchase date |
| SubTotal | REAL | Subtotal before tax |
| Tax | REAL | Tax amount |
| Total | REAL | Final total amount |
| PaymentStatus | TEXT | Payment status (Pending, Partial, Paid) |
| AmountPaid | REAL | Amount paid to supplier |
| Notes | TEXT | Additional notes |
| UserId | INTEGER | User who created purchase |
| CreatedAt | TEXT | Creation timestamp |

### 8. PurchaseItems
Individual items in a purchase order.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| PurchaseId | INTEGER | Foreign key to Purchases |
| ProductId | INTEGER | Foreign key to Products |
| ProductCode | TEXT | Product code (denormalized) |
| ProductName | TEXT | Product name (denormalized) |
| Qty | INTEGER | Quantity purchased |
| CostPrice | REAL | Unit cost price |
| Total | REAL | Line total |

### 9. Expenses
Business expenses tracking.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Category | TEXT | Expense category |
| Amount | REAL | Expense amount |
| Date | TEXT | Expense date |
| Note | TEXT | Description/notes |
| PaymentMethod | TEXT | How expense was paid |
| UserId | INTEGER | User who recorded expense |
| CreatedAt | TEXT | Creation timestamp |

### 10. Categories
Product category definitions.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Unique category name |
| Description | TEXT | Category description |
| CreatedAt | TEXT | Creation timestamp |

## Relationships

```
Users
  ??? Sales (1:many)
  ??? Purchases (1:many)
  ??? Expenses (1:many)

Products
  ??? SaleItems (1:many)
  ??? PurchaseItems (1:many)

Suppliers
  ??? Purchases (1:many)

Customers
  ??? Sales (1:many)

Sales
  ??? SaleItems (1:many)

Purchases
  ??? PurchaseItems (1:many)
```

## Indexes

Performance indexes are created on:
- Products: Code, Barcode, Category
- Sales: Date, UserId, CustomerId, InvoiceNumber
- SaleItems: SaleId, ProductId
- Purchases: SupplierId, Date
- PurchaseItems: PurchaseId
- Expenses: Date, Category

## C# Models

All database tables have corresponding C# model classes in the `Models/` folder:
- `User.cs`
- `Product.cs`
- `Supplier.cs`
- `Customer.cs`
- `Sale.cs`
- `SaleItem.cs`
- `Purchase.cs`
- `PurchaseItem.cs`
- `Expense.cs`
- `Category.cs`

## Common Queries

### Get Low Stock Products
```sql
SELECT * FROM Products WHERE StockQty <= MinStockLevel;
```

### Today's Sales Summary
```sql
SELECT COUNT(*) as TotalSales, SUM(Total) as TotalAmount
FROM Sales 
WHERE DATE(Date) = DATE('now');
```

### Top Selling Products
```sql
SELECT p.Name, SUM(si.Qty) as TotalQty, SUM(si.Total) as Revenue
FROM SaleItems si
JOIN Products p ON si.ProductId = p.Id
GROUP BY si.ProductId
ORDER BY Revenue DESC
LIMIT 10;
```

### Monthly Revenue
```sql
SELECT strftime('%Y-%m', Date) as Month, SUM(Total) as Revenue
FROM Sales
GROUP BY Month
ORDER BY Month DESC;
```
