using System;

namespace FitnessTracker.Domain
{
    public class WorkoutEntry
    {
        public Guid WorkoutId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public WorkoutType Type { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}