using workoutTracker.Data;
using workoutTracker.models;

namespace workoutTracker.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly AppDbContext _db;

        public ExerciseRepository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(Exercise exercise)
        {
            _db.Exercises.Add(exercise);
            _db.SaveChanges();
        }

        public List<Exercise> GetAll()
        {
            return _db.Exercises.ToList();
        }
        public void Delete(Exercise exercise)
        {
            _db.Exercises.Remove(exercise);
            _db.SaveChanges();
        }
        public void Update(Exercise exercise)
        {
            _db.Exercises.Update(exercise);
            _db.SaveChanges();
        }
    }
}