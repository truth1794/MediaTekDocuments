using MediaTekDocuments.model;
using MediaTekDocuments.dal;

namespace MediaTekDocuments.controller
{

    /// <summary>
    /// Controleur pour la fenêtre d'authentification
    /// Gère les interactions entre la vue et le modèle
    /// Récupère les identifiants d'un utilisateur et vérifie son mot de passe pour l'authentification.
    /// </summary>
    public class FrmAuthentificationController
    {
        /// <summary>
        /// Objet d'accès aux données
        /// </summary>
        private readonly Access access;

        /// <summary>
        /// Récupération de l'instance unique d'accès aux données
        /// </summary>
        public FrmAuthentificationController()
        {
            access = Access.GetInstance();
        }

        /// <summary>
        /// récupere les identifiants d'un utilisateur
        /// </summary>
        /// <param name="login">Identifiant de l'utilisateur</param>
        /// <param name="pwd">Mot de passe de l'utilisateur</param>
        /// <returns>True si utilisateur et mot de passe trouvés</returns>
        public Service GetUser(string login, string pwd)
        {
            User utilisateur = access.GetUser(login, pwd);

            if (utilisateur != null && utilisateur.Pwd.Equals(pwd))
            {
                return new Service(utilisateur.IdService, utilisateur.Libelle);
            }
            return null;
        }
    }
}
