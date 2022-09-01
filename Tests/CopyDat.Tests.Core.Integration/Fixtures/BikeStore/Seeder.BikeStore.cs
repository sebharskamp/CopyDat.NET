using Bogus;
using CopyDat.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    internal partial class Seeder
    {
        internal static Action<ModelBuilder> GetBikeStore()
        {

            var brandIds = 1;
            var brands = new Faker<Brands>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.BrandId, f => brandIds++)
                .RuleFor(m => m.BrandName, f => f.Name.JobArea())
                .Generate(50);

            var catIds = 1;
            var categories = new Faker<Categories>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.CategoryId, f => catIds++)
                .RuleFor(m => m.CategoryName, f => f.Commerce.Categories(1).First())
                .Generate(100);

            var cusIds = 1;
            var customers = new Faker<Customers>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.CustomerId, f => cusIds++)
                .RuleFor(m => m.Email, f => f.Person.Email)
                .RuleFor(m => m.ZipCode, f => f.Address.ZipCode())
                .RuleFor(m => m.City, f => f.Address.City())
                .RuleFor(m => m.FirstName, f => f.Person.FirstName)
                .RuleFor(m => m.LastName, f => f.Person.LastName)
                .RuleFor(m => m.Phone, f => f.Person.Phone)
                .RuleFor(m => m.State, f => f.Address.State())
                .RuleFor(m => m.Street, f => f.Address.StreetName())
                .Generate(500);

            var storeIds = 1;
            var stores = new Faker<Stores>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.StoreId, f => storeIds++)
                .RuleFor(m => m.ZipCode, f => f.Address.ZipCode())
                .RuleFor(m => m.City, f => f.Address.City())
                .RuleFor(m => m.State, f => f.Address.State())
                .RuleFor(m => m.Street, f => f.Address.StreetName())
                .RuleFor(m => m.StoreName, f => f.Company.CompanyName())
                .RuleFor(m => m.Email, f => f.Person.Email)
                .RuleFor(m => m.Phone, f => f.Person.Phone)
                .Generate(10);

            var staffIds = 1;
            var staffs = new Faker<Staffs>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.StaffId, f => staffIds++)
                .RuleFor(m => m.FirstName, f => f.Person.FirstName)
                .RuleFor(m => m.LastName, f => f.Person.LastName)
                .RuleFor(m => m.Phone, f => f.Person.Phone)
                .RuleFor(m => m.Email, f => f.Person.Email)
                .RuleFor(m => m.Active, (byte)1)
                .RuleFor(m => m.StoreId, f => f.PickRandom(stores).StoreId)
                .Generate(50);

            var orderIds = 1;
            var orders = new Faker<Orders>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.OrderId, f => orderIds++)
                .RuleFor(m => m.OrderStatus, f => f.Random.Byte())
                .RuleFor(m => m.OrderDate, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2022, 1, 1)))
                .RuleFor(m => m.ShippedDate, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2022, 1, 1)))
                .RuleFor(m => m.RequiredDate, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2022, 1, 1)))
                .RuleFor(m => m.CustomerId, f => f.PickRandom(customers).CustomerId)
                .RuleFor(m => m.StoreId, f => f.PickRandom(stores).StoreId)
                .RuleFor(m => m.StaffId, f => f.PickRandom(staffs).StaffId)
                .Generate(100);

            var productIds = 1;
            var products = new Faker<Products>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.ProductId, f => productIds++)
                .RuleFor(m => m.ProductName, f => f.Commerce.ProductName())
                .RuleFor(m => m.BrandId, f => f.PickRandom(brands).BrandId)
                .RuleFor(m => m.CategoryId, f => f.PickRandom(categories).CategoryId)
                .RuleFor(m => m.ModelYear, f => (short)f.Random.Number(2014, 2022))
                .RuleFor(m => m.ListPrice, f => decimal.Parse(f.Commerce.Price(1, 199, 2)))
                .Generate(250);

            var stocks = new Faker<Stocks>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.StoreId, f => f.PickRandom(stores).StoreId)
                .RuleFor(m => m.ProductId, f => f.PickRandom(products).ProductId)
                .RuleFor(m => m.Quantity, f => f.Random.Number(0, 30));


            var orderItemsIds = 1;
            var orderItems = new Faker<OrderItems>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.ItemId, f => orderItemsIds++)
                .RuleFor(m => m.ListPrice, f => decimal.Parse(f.Commerce.Price(1, 199, 2)))
                .RuleFor(m => m.Quantity, f => f.Random.Number(1, 10))
                .RuleFor(m => m.OrderId, f => f.PickRandom(orders).OrderId)
                .RuleFor(m => m.ProductId, f => f.PickRandom(products).ProductId)
                .Generate(1000);

            // generate 1000 items

            return modelBuilder =>
            {
                modelBuilder
                    .Entity<Brands>()
                    .HasData(brands);
                modelBuilder
                    .Entity<Categories>()
                    .HasData(categories);
                modelBuilder
                    .Entity<Customers>()
                    .HasData(customers);
                modelBuilder
                    .Entity<Stores>()
                    .HasData(stores);
                modelBuilder
                    .Entity<Staffs>()
                    .HasData(staffs);
                modelBuilder
                    .Entity<Orders>()
                    .HasData(orders);
                modelBuilder
                    .Entity<Products>()
                    .HasData(products);
                modelBuilder
                    .Entity<OrderItems>()
                    .HasData(orderItems);
                modelBuilder
                    .Entity<Stocks>()
                    .HasData(stocks.AsCleanRelationTable(1000, s => s.StoreId, s => s.ProductId));
            };
        }
    }
}

public static class FakerExtensions
{
    public static List<T> AsCleanRelationTable<T, TProp1, TProp2>(this Faker<T> fakerSet, int numToSeed, Expression<Func<T, TProp1>> propertyOne, Expression<Func<T, TProp2>> propertyTwo) where T : class
    {
        return fakerSet.Generate(numToSeed).GroupBy(c => new { propertyOne, propertyTwo }).Select(c => c.FirstOrDefault()).ToList();
    }
}