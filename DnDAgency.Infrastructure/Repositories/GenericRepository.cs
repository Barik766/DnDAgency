using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        // Проверяем состояние сущности
        var entry = _context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            _dbSet.Update(entity);
        }
        // Если сущность уже отслеживается, просто сохраняем изменения

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}