using System;
using System.Collections.Generic;
using MediaTekDocuments.model;
using MediaTekDocuments.manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Serilog;
using Serilog.Formatting.Json;
using System.Diagnostics;

namespace MediaTekDocuments.dal
{
    /// <summary>
    /// Classe d'accès aux données
    /// </summary>
    public class Access
    {
        /// <summary>
        /// adresse de l'API
        /// </summary>
        //private static readonly string uriApi = "https://mediatek-documents-dhaussy.com";
        private static readonly string uriApi = "http://localhost/rest_mediatekdocuments/";
        /// <summary>
        /// nom de connexion à la bdd
        /// </summary>
        private static readonly string connectionName = "MediaTekDocuments.Properties.Settings.mediatek86ConnectionString";

        
        /// <summary>
        /// instance unique de la classe
        /// </summary>
        private static Access instance = null;
        /// <summary>
        /// instance de ApiRest pour envoyer des demandes vers l'api et recevoir la réponse
        /// </summary>
        private readonly ApiRest api = null;
        /// <summary>
        /// méthode HTTP pour select
        /// </summary>
        private const string GET = "GET";
        /// <summary>
        /// méthode HTTP pour insert
        /// </summary>
        private const string POST = "POST";
        /// <summary>
        /// méthode HTTP pour update
        /// </summary>
        private const string PUT = "PUT";
        /// <summary>
        /// méthode HTTP pour delete
        /// </summary>
        private const string DELETE = "DELETE";

        /// <summary>
        /// Méthode privée pour créer un singleton
        /// initialise l'accès à l'API
        /// </summary>
        private Access()
        {
            String authenticationString;
            try
            {
                //authenticationString = GetConnectionStringByName(connectionName);
                authenticationString = "admin:adminpwd";
                api = ApiRest.GetInstance(uriApi, authenticationString);
            }
            catch (Exception e)
            {
                Log.Fatal("Access.Access catch connectionString={0} erreur={1}", connectionName, e.Message);
                Environment.Exit(0);
            }

        }

        /// <summary>
        /// Instance Access
        /// </summary>
        /// <returns>instance unique de la classe</returns>
        public static Access GetInstance()
        {
            if (instance == null)
            {
                instance = new Access();
            }
            return instance;
        }

        /// <summary>
        /// Retourne tous les genres à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Genre</returns>
        public List<Categorie> GetAllGenres()
        {
            IEnumerable<Genre> lesGenres = TraitementRecup<Genre>(GET, "genre");
            return new List<Categorie>(lesGenres);
        }

        /// <summary>
        /// Retourne tous les rayons à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Rayon</returns>
        public List<Categorie> GetAllRayons()
        {
            IEnumerable<Rayon> lesRayons = TraitementRecup<Rayon>(GET, "rayon");
            return new List<Categorie>(lesRayons);
        }

        /// <summary>
        /// Retourne toutes les catégories de public à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Public</returns>
        public List<Categorie> GetAllPublics()
        {
            IEnumerable<Public> lesPublics = TraitementRecup<Public>(GET, "public");
            return new List<Categorie>(lesPublics);
        }

        /// <summary>
        /// Retourne toutes les livres à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Livre</returns>
        public List<Livre> GetAllLivres()
        {
            List<Livre> lesLivres = TraitementRecup<Livre>(GET, "livre");
            return lesLivres;
        }

        /// <summary>
        /// Retourne toutes les dvd à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Dvd</returns>
        public List<Dvd> GetAllDvd()
        {
            List<Dvd> lesDvd = TraitementRecup<Dvd>(GET, "dvd");
            return lesDvd;
        }

        /// <summary>
        /// Retourne toutes les revues à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Revue</returns>
        public List<Revue> GetAllRevues()
        {
            List<Revue> lesRevues = TraitementRecup<Revue>(GET, "revue");
            return lesRevues;
        }


        /// <summary>
        /// Retourne les exemplaires d'une revue
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplairesRevue(string idDocument)
        {
            List<Exemplaire> lesExemplaires = TraitementRecup<Exemplaire>(GET, "exemplaire/" + idDocument);
            return lesExemplaires;
        }

        /// <summary>
        /// Retourne les exemplaires d'un document
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplairesDocument(string idDocument)
        {
            List<Exemplaire> lesExemplairesDocument = TraitementRecup<Exemplaire>(GET, "exemplairesdocument/" + idDocument);
            return lesExemplairesDocument;
        }

        /// <summary>
        /// Retourne les suivis d'un document
        /// </summary>
        /// <returns>Liste d'objets Suivi</returns>
        public List<Suivi> GetAllSuivis()
        {
            List<Suivi> lesSuivis = TraitementRecup<Suivi>(GET, "suivi");
            return lesSuivis;
        }

        /// <summary>
        /// Retourne les commandes
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets CommandeDocument</returns>

        public List<Commande> GetCommandes(string idDocument)
        {
            List<Commande> lesCommandes = TraitementRecup<Commande>(GET, "commande/" + idDocument);
            return lesCommandes;
        }

        /// <summary>
        /// Retourne les commandes des documents
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets CommandeDocument</returns>

        public List<CommandeDocument> GetCommandesDocument(string idDocument)
        {
            List<CommandeDocument> lesCommandesDocument = TraitementRecup<CommandeDocument>(GET, "commandedocument/" + idDocument);
            return lesCommandesDocument;
        }

        /// <summary>
        /// Retourne les documents
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets Document</returns>
        public List<Document> GetAllDocuments(string idDocument)
        {
            List<Document> lesDocuments = TraitementRecup<Document>(GET, "document/" + idDocument);
            return lesDocuments;
        }

        /// <summary>
        /// Retourne les abonnements d'une revue
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets Abonnement</returns>
        public List<Abonnement> GetAbonnementRevue(string idDocument)
        {
            List<Abonnement> lesAbonnementsRevue = TraitementRecup<Abonnement>(GET, "abonnement/" + idDocument);
            return lesAbonnementsRevue;
        }

        /// <summary>
        /// Retourne les abonnements arrivants à échéance dans 30 jours
        /// </summary>
        /// <returns></returns>
        public List<Abonnement> GetAbonnementsEcheance()
        {
            List<Abonnement> lesAbonnementsAEcheance = TraitementRecup<Abonnement>(GET, "abonnementsecheance");
            return lesAbonnementsAEcheance;
        }

        /// <summary>
        /// Retourne les états d'un document
        /// </summary>
        /// <returns>Liste d'objets Etat</returns>
        public List<Etat> GetAllEtatsDocument()
        {
            List<Etat> lesEtats = TraitementRecup<Etat>(GET, "etat");
            return lesEtats;
        }

        /// <summary>
        /// ecriture d'un exemplaire en base de données
        /// </summary>
        /// <param name="exemplaire">exemplaire à insérer</param>
        /// <returns>true si l'insertion a pu se faire (retour != null)</returns>
        public bool CreateExemplaire(Exemplaire exemplaire)
        {
            String jsonExemplaire = JsonConvert.SerializeObject(exemplaire, new CustomDateTimeConverter());
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Exemplaire> liste = TraitementRecup<Exemplaire>(POST, "exemplaire/" + jsonExemplaire);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.CreateExemplaire catch jsonExemplaire={0} erreur={1} ", jsonExemplaire, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Ecriture d'une commande en base de données
        /// </summary>
        /// <param name="commande">Objet de type Commande à insérer</param>
        /// <returns>True si l'insertion a pu se faire</returns>
        public bool CreateCommande(Commande commande)
        {
            String jsonCreateCommande = JsonConvert.SerializeObject(commande, new CustomDateTimeConverter());
            Console.WriteLine("jsonCreateCommande " + jsonCreateCommande);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Commande> liste = TraitementRecup<Commande>(POST, "commande/" + jsonCreateCommande);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.CreateCommande catch jsonCreateCommande={0} erreur={1} ", jsonCreateCommande, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Ecriture d'une commande de document en base de données
        /// </summary>
        /// <param name="id">Id de la commande de document à insérer</param>
        /// <param name="nbExemplaire">Nombre d'exemplaires de la commande de document</param>
        /// <param name="idLivreDvd">Id du livreDvd correspondant à la commande de document</param>
        /// <param name="idSuivi">Id de l'étape de suivi de la commande de document</param>
        /// <returns>True si l'insertion a pu se faire</returns>
        public bool CreateCommandeDocument(string id, int nbExemplaire, string idLivreDvd, string idSuivi)
        {
            String jsonCreateCommandeDocument = "{ \"id\" : \"" + id + "\", \"nbExemplaire\" : \"" + nbExemplaire + "\", \"idLivreDvd\" : \"" + idLivreDvd + "\", \"idSuivi\" : \"" + idSuivi + "\"}";
            Console.WriteLine("jsonCreateCommandeDocument" + jsonCreateCommandeDocument);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<CommandeDocument> liste = TraitementRecup<CommandeDocument>(POST, "commandedocument/" + jsonCreateCommandeDocument);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.CreateCommandeDocument catch jsonCreateCommandeDocument={0} erreur={1} ", jsonCreateCommandeDocument, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Modification de l'étape de suivi d'une commande de document en base de données
        /// </summary>
        /// <param name="id">Id de la commande de document à modifier</param>
        /// <param name="idSuivi">Id de l'étape de suivi</param>
        /// <returns>True si la modification a pu se faire</returns>
        public bool EditSuiviCommandeDocument(string id, string idSuivi)
        {
            String jsonEditSuiviCommandeDocument = "{ \"id\" : \"" + id + "\", \"idSuivi\" : \"" + idSuivi + "\"}";
            Console.WriteLine("jsonEditSuiviCommandeDocument" + jsonEditSuiviCommandeDocument);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<CommandeDocument> liste = TraitementRecup<CommandeDocument>(PUT, "commandedocument/" + id + "/" + jsonEditSuiviCommandeDocument);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.EditSuiviCommandeDocument catch jsonEditSuiviCommandeDocument={0} erreur={1} ", jsonEditSuiviCommandeDocument, ex.Message);

            }
            return false;
        }

        /// <summary>
        /// Suppression d'une commande de document en base de données
        /// </summary>
        /// <param name="commandesDocument">Objet de type CommandeDocument à supprimer</param>
        /// <returns>True si la suppression a pu se faire</returns>
        public bool DeleteCommandeDocument(CommandeDocument commandesDocument)
        {
            String jsonDeleteCommandeDocument = "{\"id\":\"" + commandesDocument.Id + "\"}";
            Console.WriteLine("jsonDeleteCommandeDocument=" + jsonDeleteCommandeDocument);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<CommandeDocument> liste = TraitementRecup<CommandeDocument>(DELETE, "commandedocument/" + jsonDeleteCommandeDocument);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.DeleteCommandeDocument catch jsonDeleteCommandeDocument={0} erreur={1} ", jsonDeleteCommandeDocument, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Ecriture d'un abonnement à une revue en base de données
        /// </summary>
        /// <param name="id">Id de l'abonnement à une revue à insérer</param>
        /// <param name="dateFinAbonnement">Date de fin d'abonnement à une revue</param>
        /// <param name="idRevue">Id de la revue concernée par l'abonnement</param>
        /// <returns>True si l'insertion a pu se faire</returns>
        public bool CreateAbonnementRevue(string id, DateTime dateFinAbonnement, string idRevue)
        {
            String jsonDateCommande = JsonConvert.SerializeObject(dateFinAbonnement, new CustomDateTimeConverter());
            String jsonCreateAbonnementRevue = "{\"id\":\"" + id + "\", \"dateFinAbonnement\" : " + jsonDateCommande + ", \"idRevue\" :  \"" + idRevue + "\"}";
            Console.WriteLine("jsonCreateAbonnementRevue" + jsonCreateAbonnementRevue);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Abonnement> liste = TraitementRecup<Abonnement>(POST, "abonnement/" + jsonCreateAbonnementRevue);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.CreateAbonnementRevue catch jsonCreateAbonnementRevue={0} erreur={1} ", jsonCreateAbonnementRevue, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Suppression d'un abonnement de revue en base de données
        /// </summary>
        /// <param name="abonnement">Objet de type Abonnement à supprimer</param>
        /// <returns>True si la suppression a pu se faire</returns>
        public bool DeleteAbonnementRevue(Abonnement abonnement)
        {
            String jsonDeleteAbonnementRevue = "{\"id\":\"" + abonnement.Id + "\"}";
            Console.WriteLine("jsonDeleteAbonnementRevue=" + jsonDeleteAbonnementRevue);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Abonnement> liste = TraitementRecup<Abonnement>(DELETE, "abonnement/" + jsonDeleteAbonnementRevue);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.DeleteAbonnementRevue catch jsonDeleteAbonnementRevue={0} erreur={1} ", jsonDeleteAbonnementRevue, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// modification de l'état d'un exemplaire en base de données
        /// </summary>
        /// <param name="exemplaire">Objet de type Exemplaire à modifier</param>
        /// <returns>true si la modification a pu se faire </returns>
        public bool EditEtatExemplaireDocument(Exemplaire exemplaire)
        {
            String jsonEditEtatExemplaireDocument = JsonConvert.SerializeObject(exemplaire, new CustomDateTimeConverter());
            Console.WriteLine("jsonEditEtatExemplaireDocument" + jsonEditEtatExemplaireDocument);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Exemplaire> liste = TraitementRecup<Exemplaire>(PUT, "exemplairesdocument/" + exemplaire.Numero + "/" + jsonEditEtatExemplaireDocument); // Modification de la requête
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.EditEtatExemplaireDocument catch jsonEditEtatExemplaireDocument={0} erreur={1} ", jsonEditEtatExemplaireDocument, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// suppression d'un exemplaire de document en base de données
        /// </summary>
        /// <param name="exemplaire">Objet de type Exemplaire à supprimer</param>
        /// <returns>True si la suppression a pu se faire</returns>
        public bool DeleteExemplaireDocument(Exemplaire exemplaire)
        {
            String jsonDeleteExemplaireDocument = "{\"id\":\"" + exemplaire.Id + "\",\"numero\":\"" + exemplaire.Numero + "\"}";
            Console.WriteLine("jsonDeleteExemplaireDocument" + jsonDeleteExemplaireDocument);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Exemplaire> liste = TraitementRecup<Exemplaire>(DELETE, "exemplaire/" + jsonDeleteExemplaireDocument);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.DeleteExemplaireDocument catch jsonDeleteExemplaireDocument={0} erreur={1} ", jsonDeleteExemplaireDocument, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Ecriture d'un exemplaire de revue en base de données
        /// </summary>
        /// <param name="id">Id du document correspondant à l'exemplaire d'une revue à insérer</param>
        /// <param name="numero">Numéro de l'exemplaire d'une revue à insérer</param>
        /// <param name="dateAchat">Date d'achat de l'exemplaire d'une revue</param>
        /// <param name="photo">Photo de l'exemplaire de la revue</param>
        /// <param name="idEtat">Id de l'état d'usure de l'exemplaire d'une revue</param>
        /// <returns>True si l'insertion a pu se faire</returns>
        public bool CreateExemplaireRevue(string id, int numero, DateTime dateAchat, string photo, string idEtat)
        {
            String jsonDateAchat = JsonConvert.SerializeObject(dateAchat, new CustomDateTimeConverter());
            String jsonCreateExemplaireRevue = "{\"id\":\"" + id + "\", \"numero\":\"" + numero + "\", \"dateAchat\" : " + jsonDateAchat + ", \"photo\" :  \"" + photo + "\" , \"idEtat\" :  \"" + idEtat + "\"}";
            Console.WriteLine("jsonCreateExemplaireRevue" + jsonCreateExemplaireRevue);
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Abonnement> liste = TraitementRecup<Abonnement>(POST, "exemplaire/" + jsonCreateExemplaireRevue);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.CreateExemplaireRevue catch jsonCreateExemplaireRevue={0} erreur={1} ", jsonCreateExemplaireRevue, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Récupère les informations d'identification de l'utilisateur
        /// </summary>
        /// <param name="login">Identifiant de l'utilisateur</param>
        /// <param name="password">Mot de passe de l'utilisateur</param>
        /// <returns>True si l'utilisateur est trouvé</returns>
        public User GetUser(string login, string password)
        {
            try
            {
                List<User> liste = TraitementRecup<User>(GET, "user/" + login);
                
                if (liste == null || liste.Count == 0)
                {
                    return null;
                }
                User utilisateur = liste[0];
                if (utilisateur.Pwd != password)
                {
                    
                    return null;
                }
                return utilisateur;
            }
            catch (Exception ex)
            {
                Log.Error("Access.GetUser catch login={0} erreur={1}", login, ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Traitement de la récupération du retour de l'api, avec conversion du json en liste pour les select (GET)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methode">verbe HTTP (GET, POST, PUT, DELETE)</param>
        /// <param name="message">information envoyée</param>
        /// <returns>liste d'objets récupérés (ou liste vide)</returns>
        private List<T> TraitementRecup<T>(String methode, String message)
        {
            List<T> liste = new List<T>();
            try
            {
                
                JObject retour = api.RecupDistant(methode, message);
                String code = (String)retour["code"];
                if (code.Equals("200"))
                {
                    if (methode.Equals(GET))
                    {
                        Debug.WriteLine(retour["result"][0]);
                        JArray results = (JArray)retour["result"];
                        foreach (var result in results)
                        {
                            T data = result.ToObject<T>();
                            liste.Add(data);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("code erreur = " + code + ", message = " + (String)retour["message"]);
                    //Log.Error("Access.TraitementRecup catch code={0} erreur={1} ", code);
                }
            }
            catch (Exception e)
            {
                Log.Error("Access.TraitementRecup catch liste={0} erreur={1}", liste, e.Message);
                //Environment.Exit(0);
            }
            return liste;
        }

        /// <summary>
        /// Modification du convertisseur Json pour gérer le format de date
        /// </summary>
        private sealed class CustomDateTimeConverter : IsoDateTimeConverter
        {
            public CustomDateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }
    }
}