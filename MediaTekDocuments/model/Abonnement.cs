﻿using System;

namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Abonnement hérite de Commande : contient des propriétés spécifiques aux Abonnements d'une revue
    /// </summary>
    public class Abonnement : Commande
    {
        /// <summary>
        /// Getter/Setter DateFinAbo
        /// </summary>
        public DateTime DateFinAbonnement { get; set; }

        /// <summary>
        /// Getter/Setter IdRevue
        /// </summary>
        public string IdRevue { get; set; }

        /// <summary>
        /// Getter/Setter Titre
        /// </summary>
        public string Titre { get; set; }

        /// <summary>
        /// Abonnement
        /// </summary>
        /// <param name="id">Id de l'abonnement</param>
        /// <param name="dateCommande">Date de la commande de l'abonnement</param>
        /// <param name="montant">Montant de l'abonnement</param>
        /// <param name="dateFinAbonnement">Date de fin de l'abonnement</param>
        /// <param name="idRevue">Id de la revue concernée par l'abonnement</param>
        /// <param name="titre">Titre de la revue concernée par l'abonnement</param>
        public Abonnement(string id, DateTime dateCommande, double montant, DateTime dateFinAbonnement, string idRevue, string titre) : base(id, dateCommande, montant)
        {
            this.DateFinAbonnement = dateFinAbonnement;
            this.IdRevue = idRevue;
            this.Titre = titre;
        }
    }
}
