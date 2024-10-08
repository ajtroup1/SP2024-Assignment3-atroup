﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    public class ActorMovie
    {
        public int Id { get; set; }

        [ForeignKey("Actor")]
        public int ActorId { get; set; }
        public Actor Actor { get; set; }

        [ForeignKey("Movie")]
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
