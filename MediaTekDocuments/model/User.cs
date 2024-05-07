namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier User (identification d'un utilisateur)
    /// </summary>
    public class User
    {
        /// <summary>
        /// Récupère l'identifiant de l'utilisateur
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Récupère le mot de passe de l'utilisateur
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// Récupère l'id du Service de l'utilisateur
        /// </summary>
        public string IdService { get; set; }

        /// <summary>
        /// Récupère le libelle du Service de l'utilisateur
        /// </summary>
        public string Libelle { get; set; }

        /// <summary>
        /// Retourne les infos utilisateur sous forme de string
        /// </summary>
        public override string ToString()
        {
            return $"User: {{ Login: {this.Login}, Pwd: {this.Pwd}, IdService: {this.IdService}, Service: {this.Libelle} }}"; // replace Name and Age with your actual properties
        }

        /// <summary>
        /// Initialisation d'un objet User
        /// </summary>
        /// <param name="login">Login de l'utilisateur</param>
        /// <param name="pwd">Mot de passe de l'utilisateur</param>
        /// <param name="idService">Id du service de l'utilisateur</param>
        /// <param name="libelle">Libelle du service de l'utilisateur</param>
        public User(string login, string pwd, string idService, string libelle)
        {
            this.Login = login;
            this.Pwd = pwd;
            this.IdService = idService;
            this.Libelle = libelle;
        }
    }
}