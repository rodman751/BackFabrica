namespace CapaDapper.Entidades.Productos
{
    /// <summary>
    /// Represents a product category, which may be nested under a parent category.
    /// </summary>
    public class Categoria
    {
        /// <summary>Unique identifier of the category record.</summary>
        public int Id { get; set; }
        /// <summary>Display name of the category.</summary>
        public string Nombre { get; set; }
        /// <summary>Description of the category.</summary>
        public string Descripcion { get; set; }
        /// <summary>Optional foreign key referencing a parent category for hierarchical classification.</summary>
        public int? Padre_Id { get; set; }
        /// <summary>Indicates whether the category is active.</summary>
        public bool Activo { get; set; }
    }
}
