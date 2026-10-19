using workoutTracker.models;

namespace workoutTracker.Repositories
{
    public interface IExerciseRepository
    {
        void Add(Exercise exercise);
        List<Exercise> GetAll();
        void Delete(Exercise exercise);
        void Update(Exercise exercise);
    }
}