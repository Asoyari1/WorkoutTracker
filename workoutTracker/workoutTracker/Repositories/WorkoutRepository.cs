using Microsoft.EntityFrameworkCore;
using workoutTracker.Data;
using workoutTracker.models;

namespace workoutTracker.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly AppDbContext _db;

        public WorkoutRepository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(Workout workout)
        {
            _db.Workouts.Add(workout);
            _db.SaveChanges();
        }

        public List<Workout> GetAll()
        {
            return _db.Workouts
                .Include(w => w.Exercises)
                .ToList();
        }
    }
}