﻿using PetShop.DataAccess;
using PetShop.IRepositories;
using PetShop.Models;

namespace PetShop.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbFactory dbFactory;
        private CodecampN3Context context = new CodecampN3Context();
        //private RepositoryBase<Product> productRepository;
        //private RepositoryBase<Category> categoryRepository;

        public RepositoryBase<Category> Categories {
            get
            {
                if (Categories == null)
                {
                    return new CategoryDA(context);
                }
                return Categories;
            } 
        }
        public RepositoryBase<Product> Products
        {
            get
            {
                if (Products == null)
                {
                    return new ProductDA(context);
                }
                return Products;
            }

        }

        //public UnitOfWork(CodecampN3Context context)
        //{
        //    this.context = context;

        //    Categories = new CategoryDA(context);
        //    CategoryProducts = new CategoryProductDA(context);
        //    Products = new ProductDA(context);
        //}

        public UnitOfWork(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public void Commit()
        {
            context.SaveChanges();
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
