
namespace workoutTracker.models
{
    public class Exercise
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string MuscleGroup { get; set; } = "";

        public List<Workout> Workouts { get; set; } = new();

        public Exercise()
        {
        }

        public Exercise(string name, string muscleGroup)
        {
            Name = name;
            MuscleGroup = muscleGroup;
        }
    }
}
