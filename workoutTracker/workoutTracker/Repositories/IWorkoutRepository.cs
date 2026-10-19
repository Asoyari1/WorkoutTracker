using workoutTracker.models;

namespace workoutTracker.Repositories
{
    public interface IWorkoutRepository
    {
        void Add(Workout workout);
        List<Workout> GetAll();
    }
}