
namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Categorie (réunit les informations des classes Public, Genre et Rayon)
    /// </summary>
    public class Categorie
    {
        /// <summary>
        /// Getter/Setter Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Getter/Setter Libelle
        /// </summary>
        public string Libelle { get; set; }

        /// <summary>
        /// Categorie
        /// </summary>
        /// <param name="id">Id de la catégorie</param>
        /// <param name="libelle">Libelle de la catégorie</param>
        public Categorie(string id, string libelle)
        {
            this.Id = id;
            this.Libelle = libelle;
        }

        /// <summary>
        /// Récupération du libellé pour l'affichage dans les combos
        /// </summary>
        /// <returns>Libelle</returns>
        public override string ToString()
        {
            return this.Libelle;
        }

    }
}
