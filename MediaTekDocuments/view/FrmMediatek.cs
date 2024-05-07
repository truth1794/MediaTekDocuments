using System;
using System.Windows.Forms;
using MediaTekDocuments.model;
using MediaTekDocuments.controller;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

namespace MediaTekDocuments.view

{
    /// <summary>
    /// Classe d'affichage
    /// </summary>
    public partial class FrmMediatek : Form
    {
        #region Commun
        private readonly FrmMediatekController controller;
        private readonly BindingSource bdgGenres = new BindingSource();
        private readonly BindingSource bdgPublics = new BindingSource();
        private readonly BindingSource bdgRayons = new BindingSource();

        private readonly Service service = null;

        /// <summary>
        /// Constructeur : création du contrôleur lié à ce formulaire
        /// </summary>
        public FrmMediatek(Service service)
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();
            this.service = service;
        }

        /// <summary>
        /// Affichage de l'alerte si le service de l'utilisateur est "administratif" ou "administrateur"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmNotificationAbonnement(object sender, EventArgs e)
        {
            if (service.Libelle == "administrateur" || service.Libelle == "administratif")
            {
                FrmAlerteFinAbonnement frmAlerteFinAbonnement = new FrmAlerteFinAbonnement(controller);
                frmAlerteFinAbonnement.ShowDialog();
            }
            AutorisationsAcces(service);
        }

        /// <summary>
        /// Désactive les champs selon les habilitations du service de prêts
        /// </summary>
        /// <param name="service">Service associé à l'utilisateur</param>
        private void AutorisationsAcces(Service service)
        {
            if (service.Libelle == "prêts")
            {
                tabOngletsApplication.TabPages.Remove(tabCommandesLivres);
                tabOngletsApplication.TabPages.Remove(tabCommandesDvd);
                tabOngletsApplication.TabPages.Remove(tabCommandesRevues);

                grpLivresInfos.Enabled = false;

                grpDvdInfos.Enabled = false;

                grpRevuesInfos.Enabled = false;

                txtReceptionExemplaireNumero.Enabled = false;
                dtpReceptionExemplaireDate.Enabled = false;
                txtReceptionExemplaireImage.Enabled = false;
                btnReceptionExemplaireImage.Enabled = false;
                btnReceptionExemplaireValider.Enabled = false;
            }
        }

        /// <summary>
        /// Rempli un des 3 combo (genre, public, rayon)
        /// </summary>
        /// <param name="lesCategories">liste des objets de type Genre ou Public ou Rayon</param>
        /// <param name="bdg">bindingsource contenant les informations</param>
        /// <param name="cbx">combobox à remplir</param>
        public void RemplirComboCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            bdg.DataSource = lesCategories;
            cbx.DataSource = bdg;
            if (cbx.Items.Count > 0)
            {
                cbx.SelectedIndex = -1;
            }
        }

        #endregion

        #region Onglet Livres
        private readonly BindingSource bdgLivresListe = new BindingSource();
        private List<Livre> lesLivres = new List<Livre>();

        /// <summary>
        /// Ouverture de l'onglet Livres : 
        /// appel des méthodes pour remplir le datagrid des livres et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controller.GetAllLivres();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxLivresGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxLivresPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxLivresRayons);
            RemplirCbxAjoutModifGenreLivre();
            RemplirCbxAjoutModifPublicLivre();
            RemplirCbxAjoutModifRayonLivre();
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="livres">liste de livres</param>
        private void RemplirLivresListe(List<Livre> livres)
        {
            bdgLivresListe.DataSource = livres;
            dgvLivresListe.DataSource = bdgLivresListe;
            dgvLivresListe.Columns["isbn"].Visible = false;
            dgvLivresListe.Columns["idRayon"].Visible = false;
            dgvLivresListe.Columns["idGenre"].Visible = false;
            dgvLivresListe.Columns["idPublic"].Visible = false;
            dgvLivresListe.Columns["image"].Visible = false;
            dgvLivresListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLivresListe.Columns["id"].DisplayIndex = 0;
            dgvLivresListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage du livre dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txtLivresNumRecherche.Text.Equals(""))
            {
                txtLivresTitreRecherche.Text = "";
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                Livre livre = lesLivres.Find(x => x.Id.Equals(txtLivresNumRecherche.Text));
                if (livre != null)
                {
                    List<Livre> livres = new List<Livre>() { livre };
                    RemplirLivresListe(livres);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                RemplirLivresListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des livres dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLivresTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txtLivresTitreRecherche.Text.Equals(""))
            {
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                txtLivresNumRecherche.Text = "";
                List<Livre> lesLivresParTitre;
                lesLivresParTitre = lesLivres.FindAll(x => x.Titre.ToLower().Contains(txtLivresTitreRecherche.Text.ToLower()));
                RemplirLivresListe(lesLivresParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxLivresGenres.SelectedIndex < 0 && cbxLivresPublics.SelectedIndex < 0 && cbxLivresRayons.SelectedIndex < 0
                    && txtLivresNumRecherche.Text.Equals(""))
                {
                    RemplirLivresListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations du livre sélectionné
        /// </summary>
        /// <param name="livre">le livre</param>
        private void AfficheLivresInfos(Livre livre)
        {
            txtLivresAuteur.Text = livre.Auteur;
            txtLivresCollection.Text = livre.Collection;
            txtLivresImage.Text = livre.Image;
            txtLivresIsbn.Text = livre.Isbn;
            txtLivresNumero.Text = livre.Id;
            txtLivresTitre.Text = livre.Titre;
            txtLivresGenre.Text = livre.Genre;
            txtLivresPublic.Text = livre.Public;
            txtLivresRayon.Text = livre.Rayon;
            string image = livre.Image;
            try
            {
                pcbLivresImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbLivresImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du livre
        /// </summary>
        private void VideLivresInfos()
        {
            txtLivresAuteur.Text = "";
            txtLivresCollection.Text = "";
            txtLivresImage.Text = "";
            txtLivresIsbn.Text = "";
            txtLivresNumero.Text = "";
            txtLivresGenre.Text = "";
            txtLivresPublic.Text = "";
            txtLivresRayon.Text = "";
            txtLivresTitre.Text = "";
            pcbLivresImage.Image = null;
            cbxAjoutModifGenreLivre.Text = "";
            cbxAjoutModifPublicLivre.Text = "";
            cbxAjoutModifRayonLivre.Text = "";
        }

        private void btnInfosLivreVider_Click(object sender, EventArgs e)
        {
            VideLivresInfos();
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxLivresGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresGenres.SelectedIndex >= 0)
            {
                txtLivresTitreRecherche.Text = "";
                txtLivresNumRecherche.Text = "";
                Genre genre = (Genre)cbxLivresGenres.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirLivresListe(livres);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }


        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxLivresPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresPublics.SelectedIndex >= 0)
            {
                txtLivresTitreRecherche.Text = "";
                txtLivresNumRecherche.Text = "";
                Public lePublic = (Public)cbxLivresPublics.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirLivresListe(livres);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxLivresRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresRayons.SelectedIndex >= 0)
            {
                txtLivresTitreRecherche.Text = "";
                txtLivresNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxLivresRayons.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirLivresListe(livres);
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvLivresListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLivresListe.CurrentCell != null)
            {
                try
                {
                    Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                    AfficheLivresInfos(livre);
                    VideComboAjoutModifLivres();
                }
                catch
                {
                    VideLivresZones();
                }
            }
            else
            {
                VideLivresInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des livres
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirLivresListeComplete()
        {
            RemplirLivresListe(lesLivres);
            VideLivresZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideLivresZones()
        {
            cbxLivresGenres.SelectedIndex = -1;
            cbxLivresRayons.SelectedIndex = -1;
            cbxLivresPublics.SelectedIndex = -1;
            txtLivresNumRecherche.Text = "";
            txtLivresTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideLivresZones();
            string titreColonne = dgvLivresListe.Columns[e.ColumnIndex].HeaderText;
            List<Livre> sortedList = new List<Livre>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesLivres.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesLivres.OrderBy(o => o.Titre).ToList();
                    break;
                case "Collection":
                    sortedList = lesLivres.OrderBy(o => o.Collection).ToList();
                    break;
                case "Auteur":
                    sortedList = lesLivres.OrderBy(o => o.Auteur).ToList();
                    break;
                case "Genre":
                    sortedList = lesLivres.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesLivres.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesLivres.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirLivresListe(sortedList);
        }

        /// <summary>
        /// Remplissage de la combobox du genre du livre à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des genres</returns>
        private string RemplirCbxAjoutModifGenreLivre()
        {
            List<Categorie> lesGenresLivres = controller.GetAllGenres();
            foreach (Categorie genre in lesGenresLivres)
            {
                cbxAjoutModifGenreLivre.Items.Add(genre.Libelle);
            }

            if (cbxAjoutModifGenreLivre.Items.Count > 0)
            {
                cbxAjoutModifGenreLivre.SelectedIndex = 0;
            }
            return cbxAjoutModifGenreLivre.SelectedItem?.ToString();
        }

        /// <summary>
        /// Remplissage de la combobox du public du livre à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des publics</returns>
        private string RemplirCbxAjoutModifPublicLivre()
        {
            List<Categorie> lesPublicsLivres = controller.GetAllPublics();
            foreach (Categorie lePublic in lesPublicsLivres)
            {
                cbxAjoutModifPublicLivre.Items.Add(lePublic.Libelle);
            }

            if (cbxAjoutModifPublicLivre.Items.Count > 0)
            {
                cbxAjoutModifPublicLivre.SelectedIndex = 0;
            }
            return cbxAjoutModifPublicLivre.SelectedItem?.ToString();
        }

        /// <summary>
        /// Remplissage de la combobox du rayon du livre à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des rayons</returns>
        private string RemplirCbxAjoutModifRayonLivre()
        {
            List<Categorie> lesRayonsLivres = controller.GetAllRayons();
            foreach (Categorie rayon in lesRayonsLivres)
            {
                cbxAjoutModifRayonLivre.Items.Add(rayon.Libelle);
            }

            if (cbxAjoutModifRayonLivre.Items.Count > 0)
            {
                cbxAjoutModifRayonLivre.SelectedIndex = 0;
            }
            return cbxAjoutModifRayonLivre.SelectedItem?.ToString();
        }

        /// <summary>
        /// Récupération de l'id du genre correspondant au genre de document 
        /// </summary>
        /// <param name="genre">Genre du document sélectionné</param>
        /// <returns>L'id du genre sélectionné</returns>
        private string GetIdGenreDocument(string genre)
        {
            List<Categorie> lesGenresDocument = controller.GetAllGenres();
            foreach (Categorie c in lesGenresDocument)
            {
                if (c.Libelle == genre)
                {
                    return c.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Récupération de l'id du public correspondant au public du document
        /// </summary>
        /// <param name="lePublic">Public du document sélectionné</param>
        /// <returns>L'id du public sélectionné</returns>
        private string GetIdPublicDocument(string lePublic)
        {
            List<Categorie> lesPublicsDocument = controller.GetAllPublics();
            foreach (Categorie c in lesPublicsDocument)
            {
                if (c.Libelle == lePublic)
                {
                    return c.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Récupération de l'id du rayon correspondant au rayon du document
        /// </summary>
        /// <param name="rayon">Rayon du document sélectionné</param>
        /// <returns>L'id du rayon sélectionné</returns>
        private string GetIdRayonDocument(string rayon)
        {
            List<Categorie> lesRayonsDocument = controller.GetAllRayons();
            foreach (Categorie c in lesRayonsDocument)
            {
                if (c.Libelle == rayon)
                {
                    return c.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Vide les combobox de gestion du genre, du public et du rayon d'un livre
        /// </summary>
        private void VideComboAjoutModifLivres()
        {
            cbxAjoutModifGenreLivre.Text = "";
            cbxAjoutModifPublicLivre.Text = "";
            cbxAjoutModifRayonLivre.Text = "";
        }

        

        /// <summary>
        /// Récupère l'id d'un état selon son libelle
        /// </summary>
        /// <param name="libelle">Libelle de l'état d'usure d'un exemplaire</param>
        /// <returns></returns>
        private string GetIdEtat(string libelle)
        {
            List<Etat> lesEtats = controller.GetAllEtatsDocument();
            foreach (Etat unEtat in lesEtats)
            {
                if (unEtat.Libelle == libelle)
                {
                    return unEtat.Id;
                }
            }
            return null;
        }

        
        #endregion

        #region Onglet Dvd
        private readonly BindingSource bdgDvdListe = new BindingSource();
        private List<Dvd> lesDvd = new List<Dvd>();

        /// <summary>
        /// Ouverture de l'onglet Dvds : 
        /// appel des méthodes pour remplir le datagrid des dvd et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabDvd_Enter(object sender, EventArgs e)
        {
            lesDvd = controller.GetAllDvd();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxDvdGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxDvdPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxDvdRayons);
            RemplirCbxAjoutModifGenreDvd();
            RemplirCbxAjoutModifPublicDvd();
            RemplirCbxAjoutModifRayonDvd();
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="Dvds">liste de dvd</param>
        private void RemplirDvdListe(List<Dvd> Dvds)
        {
            bdgDvdListe.DataSource = Dvds;
            dgvDvdListe.DataSource = bdgDvdListe;
            dgvDvdListe.Columns["idRayon"].Visible = false;
            dgvDvdListe.Columns["idGenre"].Visible = false;
            dgvDvdListe.Columns["idPublic"].Visible = false;
            dgvDvdListe.Columns["image"].Visible = false;
            dgvDvdListe.Columns["synopsis"].Visible = false;
            dgvDvdListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDvdListe.Columns["id"].DisplayIndex = 0;
            dgvDvdListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage du Dvd dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txtDvdNumRecherche.Text.Equals(""))
            {
                txtDvdTitreRecherche.Text = "";
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txtDvdNumRecherche.Text));
                if (dvd != null)
                {
                    List<Dvd> Dvd = new List<Dvd>() { dvd };
                    RemplirDvdListe(Dvd);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirDvdListeComplete();
                }
            }
            else
            {
                RemplirDvdListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des Dvd dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDvdTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txtDvdTitreRecherche.Text.Equals(""))
            {
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                txtDvdNumRecherche.Text = "";
                List<Dvd> lesDvdParTitre;
                lesDvdParTitre = lesDvd.FindAll(x => x.Titre.ToLower().Contains(txtDvdTitreRecherche.Text.ToLower()));
                RemplirDvdListe(lesDvdParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxDvdGenres.SelectedIndex < 0 && cbxDvdPublics.SelectedIndex < 0 && cbxDvdRayons.SelectedIndex < 0
                    && txtDvdNumRecherche.Text.Equals(""))
                {
                    RemplirDvdListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations du dvd sélectionné
        /// </summary>
        /// <param name="dvd">le dvd</param>
        private void AfficheDvdInfos(Dvd dvd)
        {
            txtDvdRealisateur.Text = dvd.Realisateur;
            txtDvdSynopsis.Text = dvd.Synopsis;
            txtDvdImage.Text = dvd.Image;
            txtDvdDuree.Text = dvd.Duree.ToString();
            txtDvdNumero.Text = dvd.Id;
            txtDvdGenre.Text = dvd.Genre;
            txtDvdPublic.Text = dvd.Public;
            txtDvdRayon.Text = dvd.Rayon;
            txtDvdTitre.Text = dvd.Titre;
            string image = dvd.Image;
            try
            {
                pcbDvdImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbDvdImage.Image = null;
            }

        }

        /// <summary>
        /// Vide les zones d'affichage des informations du dvd
        /// </summary>
        private void VideDvdInfos()
        {
            txtDvdRealisateur.Text = "";
            txtDvdSynopsis.Text = "";
            txtDvdImage.Text = "";
            txtDvdDuree.Text = "";
            txtDvdNumero.Text = "";
            txtDvdGenre.Text = "";
            txtDvdPublic.Text = "";
            txtDvdRayon.Text = "";
            txtDvdTitre.Text = "";
            pcbDvdImage.Image = null;
            cbxAjoutModifGenreDvd.Text = "";
            cbxAjoutModifPublicDvd.Text = "";
            cbxAjoutModifRayonDvd.Text = "";
        }

        private void btnInfosDvdVider_Click(object sender, EventArgs e)
        {
            VideDvdInfos();
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdGenres.SelectedIndex >= 0)
            {
                txtDvdTitreRecherche.Text = "";
                txtDvdNumRecherche.Text = "";
                Genre genre = (Genre)cbxDvdGenres.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdPublics.SelectedIndex >= 0)
            {
                txtDvdTitreRecherche.Text = "";
                txtDvdNumRecherche.Text = "";
                Public lePublic = (Public)cbxDvdPublics.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdRayons.SelectedIndex >= 0)
            {
                txtDvdTitreRecherche.Text = "";
                txtDvdNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxDvdRayons.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDvdListe.CurrentCell != null)
            {
                try
                {
                    Dvd dvd = (Dvd)bdgDvdListe.List[bdgDvdListe.Position];
                    AfficheDvdInfos(dvd);
                    VideComboAjoutModifDvd();
                }
                catch
                {
                    VideDvdZones();
                }
            }
            else
            {
                VideDvdInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des Dvd
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirDvdListeComplete()
        {
            RemplirDvdListe(lesDvd);
            VideDvdZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideDvdZones()
        {
            cbxDvdGenres.SelectedIndex = -1;
            cbxDvdRayons.SelectedIndex = -1;
            cbxDvdPublics.SelectedIndex = -1;
            txtDvdNumRecherche.Text = "";
            txtDvdTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideDvdZones();
            string titreColonne = dgvDvdListe.Columns[e.ColumnIndex].HeaderText;
            List<Dvd> sortedList = new List<Dvd>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesDvd.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesDvd.OrderBy(o => o.Titre).ToList();
                    break;
                case "Duree":
                    sortedList = lesDvd.OrderBy(o => o.Duree).ToList();
                    break;
                case "Realisateur":
                    sortedList = lesDvd.OrderBy(o => o.Realisateur).ToList();
                    break;
                case "Genre":
                    sortedList = lesDvd.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesDvd.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesDvd.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirDvdListe(sortedList);
        }

        /// <summary>
        /// Remplissage de la combobox du genre du dvd à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des genres</returns>
        private string RemplirCbxAjoutModifGenreDvd()
        {
            List<Categorie> lesGenresDvd = controller.GetAllGenres();
            foreach (Categorie genre in lesGenresDvd)
            {
                cbxAjoutModifGenreDvd.Items.Add(genre.Libelle);
            }

            if (cbxAjoutModifGenreDvd.Items.Count > 0)
            {
                cbxAjoutModifGenreDvd.SelectedIndex = 0;
            }
            return cbxAjoutModifGenreDvd.SelectedItem?.ToString();
        }

        /// <summary>
        /// Remplissage de la combobox du public du dvd à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des publics</returns>
        private string RemplirCbxAjoutModifPublicDvd()
        {
            List<Categorie> lesPublicsDvd = controller.GetAllPublics();
            foreach (Categorie lePublic in lesPublicsDvd)
            {
                cbxAjoutModifPublicDvd.Items.Add(lePublic.Libelle);
            }

            if (cbxAjoutModifPublicDvd.Items.Count > 0)
            {
                cbxAjoutModifPublicDvd.SelectedIndex = 0;
            }
            return cbxAjoutModifPublicDvd.SelectedItem?.ToString();
        }

        /// <summary>
        /// Remplissage de la combobox du rayon du dvd à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des rayons</returns>
        private string RemplirCbxAjoutModifRayonDvd()
        {
            List<Categorie> lesRayonsDvd = controller.GetAllRayons();
            foreach (Categorie rayon in lesRayonsDvd)
            {
                if (rayon.Libelle == "DVD films")
                {
                    cbxAjoutModifRayonDvd.Items.Add(rayon.Libelle);
                }
            }

            if (cbxAjoutModifRayonDvd.Items.Count > 0)
            {
                cbxAjoutModifRayonDvd.SelectedIndex = 0;
            }
            return cbxAjoutModifRayonDvd.SelectedItem?.ToString();
        }

        /// <summary>
        /// Vide les comboBox de gestion du genre, du public et du rayon d'un dvd
        /// </summary>
        private void VideComboAjoutModifDvd()
        {
            cbxAjoutModifGenreDvd.Text = "";
            cbxAjoutModifPublicDvd.Text = "";
            cbxAjoutModifRayonDvd.Text = "";
        }

        #endregion

        #region Onglet Revues
        private readonly BindingSource bdgRevuesListe = new BindingSource();
        private List<Revue> lesRevues = new List<Revue>();

        /// <summary>
        /// Ouverture de l'onglet Revues : 
        /// appel des méthodes pour remplir le datagrid des revues et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxRevuesGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxRevuesPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxRevuesRayons);
            RemplirCbxAjoutModifGenreRevue();
            RemplirCbxAjoutModifPublicRevue();
            RemplirCbxAjoutModifRayonRevue();
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="revues">Liste des revues</param>
        private void RemplirRevuesListe(List<Revue> revues)
        {
            bdgRevuesListe.DataSource = revues;
            dgvRevuesListe.DataSource = bdgRevuesListe;
            dgvRevuesListe.Columns["idRayon"].Visible = false;
            dgvRevuesListe.Columns["idGenre"].Visible = false;
            dgvRevuesListe.Columns["idPublic"].Visible = false;
            dgvRevuesListe.Columns["image"].Visible = false;
            dgvRevuesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvRevuesListe.Columns["id"].DisplayIndex = 0;
            dgvRevuesListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage de la revue dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txtRevuesNumRecherche.Text.Equals(""))
            {
                txtRevuesTitreRecherche.Text = "";
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                Revue revue = lesRevues.Find(x => x.Id.Equals(txtRevuesNumRecherche.Text));
                if (revue != null)
                {
                    List<Revue> revues = new List<Revue>() { revue };
                    RemplirRevuesListe(revues);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirRevuesListeComplete();
                }
            }
            else
            {
                RemplirRevuesListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des revues dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRevuesTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txtRevuesTitreRecherche.Text.Equals(""))
            {
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                txtRevuesNumRecherche.Text = "";
                List<Revue> lesRevuesParTitre;
                lesRevuesParTitre = lesRevues.FindAll(x => x.Titre.ToLower().Contains(txtRevuesTitreRecherche.Text.ToLower()));
                RemplirRevuesListe(lesRevuesParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxRevuesGenres.SelectedIndex < 0 && cbxRevuesPublics.SelectedIndex < 0 && cbxRevuesRayons.SelectedIndex < 0
                    && txtRevuesNumRecherche.Text.Equals(""))
                {
                    RemplirRevuesListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionné
        /// </summary>
        /// <param name="revue">la revue</param>
        private void AfficheRevuesInfos(Revue revue)
        {
            txtRevuesPeriodicite.Text = revue.Periodicite;
            txtRevuesImage.Text = revue.Image;
            txtRevuesDateMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txtRevuesNumero.Text = revue.Id;
            txtRevuesGenre.Text = revue.Genre;
            txtRevuesPublic.Text = revue.Public;
            txtRevuesRayon.Text = revue.Rayon;
            txtRevuesTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbRevuesImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbRevuesImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations de la reuve
        /// </summary>
        private void VideRevuesInfos()
        {
            txtRevuesPeriodicite.Text = "";
            txtRevuesImage.Text = "";
            txtRevuesDateMiseADispo.Text = "";
            txtRevuesNumero.Text = "";
            txtRevuesGenre.Text = "";
            txtRevuesPublic.Text = "";
            txtRevuesRayon.Text = "";
            txtRevuesTitre.Text = "";
            pcbRevuesImage.Image = null;
        }

        private void btnInfosRevuesVider_Click(object sender, EventArgs e)
        {
            VideRevuesInfos();
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesGenres.SelectedIndex >= 0)
            {
                txtRevuesTitreRecherche.Text = "";
                txtRevuesNumRecherche.Text = "";
                Genre genre = (Genre)cbxRevuesGenres.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesPublics.SelectedIndex >= 0)
            {
                txtRevuesTitreRecherche.Text = "";
                txtRevuesNumRecherche.Text = "";
                Public lePublic = (Public)cbxRevuesPublics.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesRayons.SelectedIndex >= 0)
            {
                txtRevuesTitreRecherche.Text = "";
                txtRevuesNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxRevuesRayons.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations de la revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRevuesListe.CurrentCell != null)
            {
                try
                {
                    Revue revue = (Revue)bdgRevuesListe.List[bdgRevuesListe.Position];
                    AfficheRevuesInfos(revue);
                    VideComboAjoutModifRevue();
                }
                catch
                {
                    VideRevuesZones();
                }
            }
            else
            {
                VideRevuesInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des revues
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirRevuesListeComplete()
        {
            RemplirRevuesListe(lesRevues);
            VideRevuesZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideRevuesZones()
        {
            cbxRevuesGenres.SelectedIndex = -1;
            cbxRevuesRayons.SelectedIndex = -1;
            cbxRevuesPublics.SelectedIndex = -1;
            txtRevuesNumRecherche.Text = "";
            txtRevuesTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideRevuesZones();
            string titreColonne = dgvRevuesListe.Columns[e.ColumnIndex].HeaderText;
            List<Revue> sortedList = new List<Revue>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesRevues.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesRevues.OrderBy(o => o.Titre).ToList();
                    break;
                case "Periodicite":
                    sortedList = lesRevues.OrderBy(o => o.Periodicite).ToList();
                    break;
                case "DelaiMiseADispo":
                    sortedList = lesRevues.OrderBy(o => o.DelaiMiseADispo).ToList();
                    break;
                case "Genre":
                    sortedList = lesRevues.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesRevues.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesRevues.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirRevuesListe(sortedList);
        }

        /// <summary>
        /// Remplissage de la combobox du genre de la revue à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des genres</returns>
        private string RemplirCbxAjoutModifGenreRevue()
        {
            List<Categorie> lesGenresRevue = controller.GetAllGenres();
            foreach (Categorie genre in lesGenresRevue)
            {
                cbxAjoutModifGenreRevue.Items.Add(genre.Libelle);
            }

            if (cbxAjoutModifGenreRevue.Items.Count > 0)
            {
                cbxAjoutModifGenreRevue.SelectedIndex = 0;
            }
            return cbxAjoutModifGenreRevue.SelectedItem?.ToString();
        }

        /// <summary>
        /// Remplissage de la combobox du public de la revue à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des publics</returns>
        private string RemplirCbxAjoutModifPublicRevue()
        {
            List<Categorie> lesPublicsRevue = controller.GetAllPublics();
            foreach (Categorie lePublic in lesPublicsRevue)
            {
                cbxAjoutModifPublicRevue.Items.Add(lePublic.Libelle);
            }

            if (cbxAjoutModifPublicRevue.Items.Count > 0)
            {
                cbxAjoutModifPublicRevue.SelectedIndex = 0;
            }
            return cbxAjoutModifPublicRevue.SelectedItem?.ToString();
        }

        /// <summary>
        /// Remplissage de la combobox du rayon de la revue à ajouter ou à modifier
        /// </summary>
        /// <returns>La liste des rayons</returns>
        private string RemplirCbxAjoutModifRayonRevue()
        {
            List<Categorie> lesRayonsRevue = controller.GetAllRayons();
            foreach (Categorie rayon in lesRayonsRevue)
            {
                cbxAjoutModifRayonRevue.Items.Add(rayon.Libelle);
            }

            if (cbxAjoutModifRayonRevue.Items.Count > 0)
            {
                cbxAjoutModifRayonRevue.SelectedIndex = 0;
            }
            return cbxAjoutModifRayonRevue.SelectedItem?.ToString();
        }

        /// <summary>
        /// Vide les comboBox de gestion du genre, du public et du rayon d'une revue
        /// </summary>
        private void VideComboAjoutModifRevue()
        {
            cbxAjoutModifGenreRevue.Text = "";
            cbxAjoutModifPublicRevue.Text = "";
            cbxAjoutModifRayonRevue.Text = "";
        }

        #endregion

        #region Onglet Paarutions
        private readonly BindingSource bdgExemplairesListe = new BindingSource();
        private List<Exemplaire> lesExemplaires = new List<Exemplaire>();
        const string ETATNEUF = "00001";

        /// <summary>
        /// Ouverture de l'onglet : récupère le revues et vide tous les champs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabReceptionRevue_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            txtReceptionRevueNumero.Text = "";
        }

        /// <summary>
        /// Remplit le dategrid des exemplaires avec la liste reçue en paramètre
        /// </summary>
        /// <param name="exemplaires">liste d'exemplaires</param>
        private void RemplirReceptionExemplairesListe(List<Exemplaire> exemplaires)
        {
            if (exemplaires != null)
            {
                bdgExemplairesListe.DataSource = exemplaires;
                dgvReceptionExemplairesListe.DataSource = bdgExemplairesListe;
                dgvReceptionExemplairesListe.Columns["idEtat"].Visible = false;
                dgvReceptionExemplairesListe.Columns["id"].Visible = false;
                dgvReceptionExemplairesListe.Columns["photo"].Visible = false;
                dgvReceptionExemplairesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvReceptionExemplairesListe.Columns[1].HeaderCell.Value = "Numéro";
                dgvReceptionExemplairesListe.Columns[3].HeaderCell.Value = "Date d'achat";
                dgvReceptionExemplairesListe.Columns[5].HeaderCell.Value = "Etat";
            }
            else
            {
                bdgExemplairesListe.DataSource = null;
            }
        }

        /// <summary>
        /// Recherche d'un numéro de revue et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionRechercher_Click(object sender, EventArgs e)
        {
            if (!txtReceptionRevueNumero.Text.Equals(""))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(txtReceptionRevueNumero.Text));
                if (revue != null)
                {
                    AfficheReceptionRevueInfos(revue);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                }
            }
        }

        /// <summary>
        /// Si le numéro de revue est modifié, la zone de l'exemplaire est vidée et inactive
        /// les informations de la revue son aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtReceptionRevueNumero_TextChanged(object sender, EventArgs e)
        {
            txtReceptionRevuePeriodicite.Text = "";
            txtReceptionRevueImage.Text = "";
            txtReceptionRevueDelaiMiseADispo.Text = "";
            txtReceptionRevueGenre.Text = "";
            txtReceptionRevuePublic.Text = "";
            txtReceptionRevueRayon.Text = "";
            txtReceptionRevueTitre.Text = "";
            pcbReceptionRevueImage.Image = null;
            RemplirReceptionExemplairesListe(null);
            AccesReceptionExemplaireGroupBox(false);
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionnée et les exemplaires
        /// </summary>
        /// <param name="revue">la revue</param>
        private void AfficheReceptionRevueInfos(Revue revue)
        {
            // informations sur la revue
            txtReceptionRevuePeriodicite.Text = revue.Periodicite;
            txtReceptionRevueImage.Text = revue.Image;
            txtReceptionRevueDelaiMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txtReceptionRevueNumero.Text = revue.Id;
            txtReceptionRevueGenre.Text = revue.Genre;
            txtReceptionRevuePublic.Text = revue.Public;
            txtReceptionRevueRayon.Text = revue.Rayon;
            txtReceptionRevueTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbReceptionRevueImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbReceptionRevueImage.Image = null;
            }
            // affiche la liste des exemplaires de la revue
            AfficheReceptionExemplairesRevue();
        }

        /// <summary>
        /// Récupère et affiche les exemplaires d'une revue
        /// </summary>
        private void AfficheReceptionExemplairesRevue()
        {
            string idDocument = txtReceptionRevueNumero.Text;
            lesExemplaires = controller.GetExemplairesDocument(idDocument);
            RemplirReceptionExemplairesListe(lesExemplaires);
            AccesReceptionExemplaireGroupBox(true);
        }

        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la réception d'un exemplaire
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces">true ou false</param>
        private void AccesReceptionExemplaireGroupBox(bool acces)
        {
            grpReceptionExemplaire.Enabled = acces;
            txtReceptionExemplaireImage.Text = "";
            txtReceptionExemplaireNumero.Text = "";
            pcbReceptionExemplaireImage.Image = null;
            dtpReceptionExemplaireDate.Value = DateTime.Now;
        }

        /// <summary>
        /// Recherche image sur disque (pour l'exemplaire à insérer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                // positionnement à la racine du disque où se trouve le dossier actuel
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txtReceptionExemplaireImage.Text = filePath;
            try
            {
                pcbReceptionExemplaireImage.Image = Image.FromFile(filePath);
            }
            catch
            {
                pcbReceptionExemplaireImage.Image = null;
            }
        }

        /// <summary>
        /// Enregistrement du nouvel exemplaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireValider_Click(object sender, EventArgs e)
        {
            if (!txtReceptionExemplaireNumero.Text.Equals(""))
            {
                try
                {
                    int numero = int.Parse(txtReceptionExemplaireNumero.Text);
                    DateTime dateAchat = dtpReceptionExemplaireDate.Value;
                    string photo = txtReceptionExemplaireImage.Text;
                    string idEtat = ETATNEUF;
                    string idDocument = txtReceptionRevueNumero.Text;
                    string libelle = "";
                    Exemplaire exemplaire = new Exemplaire(numero, dateAchat, photo, idEtat, idDocument, libelle);
                    if (controller.CreateExemplaireRevue(idDocument, numero, dateAchat, photo, idEtat))
                    {
                        AfficheReceptionExemplairesRevue();
                    }
                    else
                    {
                        MessageBox.Show("numéro de publication déjà existant", "Erreur");
                    }
                }
                catch
                {
                    MessageBox.Show("le numéro de parution doit être numérique", "Information");
                    txtReceptionExemplaireNumero.Text = "";
                    txtReceptionExemplaireNumero.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de parution obligatoire", "Information");
            }
        }

        /// <summary>
        /// Tri sur une colonne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExemplairesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvReceptionExemplairesListe.Columns[e.ColumnIndex].HeaderText;
            List<Exemplaire> sortedList = new List<Exemplaire>();
            switch (titreColonne)
            {
                case "Numero":
                    sortedList = lesExemplaires.OrderBy(o => o.Numero).Reverse().ToList();
                    break;
                case "DateAchat":
                    sortedList = lesExemplaires.OrderBy(o => o.DateAchat).Reverse().ToList();
                    break;
                case "Etat":
                    sortedList = lesExemplaires.OrderBy(o => o.Libelle).ToList();
                    break;
            }
            RemplirReceptionExemplairesListe(sortedList);
        }

        /// <summary>
        /// affichage de l'image de l'exemplaire suite à la sélection d'un exemplaire dans la liste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvReceptionExemplairesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.CurrentCell != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesListe.List[bdgExemplairesListe.Position];
                string image = exemplaire.Photo;
                try
                {
                    pcbReceptionExemplaireRevueImage.Image = Image.FromFile(image);
                }
                catch
                {
                    pcbReceptionExemplaireRevueImage.Image = null;
                }
            }
            else
            {
                pcbReceptionExemplaireRevueImage.Image = null;
            }
        }


        /// <summary>
        /// Suppression d'un exemplaire de revue dans la bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExemplaireRevueDelete_Click(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.SelectedRows.Count > 0)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesListe.List[bdgExemplairesListe.Position];
                if (MessageBox.Show("Voulez-vous supprimer l'exemplaire " + exemplaire.Numero + " de la revue " + exemplaire.Id + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    controller.DeleteExemplaireDocument(exemplaire);
                    MessageBox.Show("L'exemplaire " + exemplaire.Numero + " a bien été supprimé.", "Information");
                    AfficheReceptionExemplairesRevue();
                }
            }
            else
            {
                MessageBox.Show("Une ligne doit être sélectionnée.", "Information");
            }
        }

        #endregion

        #region Onglet CommandesLivres
        private readonly BindingSource bdgCommandesLivre = new BindingSource();
        private List<CommandeDocument> cmdDoc = new List<CommandeDocument>();
        private List<Suivi> lesSuivis = new List<Suivi>();

        /// <summary>
        /// Ouverture de l'onglet Commandes de livres :
        /// appel des méthodes pour remplir le datagrid des commandes de livre et du combo "suivi"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandesLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controller.GetAllLivres();
            lesSuivis = controller.GetAllSuivis();
            gbxInfosCommandeLivre.Enabled = false;
            gbxEtapeSuivi.Enabled = false;
        }

        /// <summary>
        /// Masque la groupBox des suivis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gbxInfosCommandeLivre_Enter(object sender, EventArgs e)
        {
            gbxEtapeSuivi.Enabled = false;
        }

        /// <summary>
        /// Affiche la groupBox des suivis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInfosCommandeLivreAnnuler_Click(object sender, EventArgs e)
        {
            gbxEtapeSuivi.Enabled = true;
            gbxInfosCommandeLivre.Enabled = false;
        }

        /// <summary>
        /// Masque la groupBox des informations de commande et le numéro de recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gbxEtapeSuivi_Enter(object sender, EventArgs e)
        {
            gbxInfosCommandeLivre.Enabled = false;
            txtCommandesLivresNumRecherche.Enabled = false;
        }

        /// <summary>
        /// Affiche la groupBox des commandes et le numéro de recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEtapeSuiviAnnuler_Click(object sender, EventArgs e)
        {
            gbxEtapeSuivi.Enabled = false;
            gbxInfosCommandeLivre.Enabled = true;
            txtCommandesLivresNumRecherche.Enabled = true;
        }

        /// <summary>
        /// Remplissage la liste
        /// </summary>
        /// <param name="cmdDoc">Liste des commandes d'un document</param>
        private void RemplirListeCommandesLivres(List<CommandeDocument> cmdDoc)
        {
            if (cmdDoc != null)
            {
                bdgCommandesLivre.DataSource = cmdDoc;
                dgvCommandesLivre.DataSource = bdgCommandesLivre;
                dgvCommandesLivre.Columns["id"].Visible = false;
                dgvCommandesLivre.Columns["idLivreDvd"].Visible = false;
                dgvCommandesLivre.Columns["idSuivi"].Visible = false;
                dgvCommandesLivre.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvCommandesLivre.Columns["dateCommande"].DisplayIndex = 4;
                dgvCommandesLivre.Columns["montant"].DisplayIndex = 1;
                dgvCommandesLivre.Columns[5].HeaderCell.Value = "Date de commande";
                dgvCommandesLivre.Columns[0].HeaderCell.Value = "Nombre d'exemplaires";
                dgvCommandesLivre.Columns[3].HeaderCell.Value = "Suivi";
            }
            else
            {
                bdgCommandesLivre.DataSource = null;
            }
        }

        /// <summary>
        /// Mise à jour de la liste des commandes de livre
        /// </summary>
        private void UpdateCommandesLivre()
        {
            string idDocument = txtCommandesLivresNumRecherche.Text;
            cmdDoc = controller.GetCommandesDocument(idDocument);
            RemplirListeCommandesLivres(cmdDoc);
        }

        /// <summary>
        /// Bouton gerant l'affichage du livre recherche (Commande)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchLivreCommande(object sender, EventArgs e)
        {
            if (!txtCommandesLivresNumRecherche.Text.Equals(""))
            {
                Livre livre = lesLivres.Find(x => x.Id.Equals(txtCommandesLivresNumRecherche.Text));
                if (livre != null)
                {
                    UpdateCommandesLivre();
                    gbxInfosCommandeLivre.Enabled = true;
                    ShowLivreInfo(livre);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                }
            }
            else
            {
                MessageBox.Show("Le numéro de document est obligatoire", "Information");
            }
        }

        /// <summary>
        /// Affichage des informations du livre
        /// </summary>
        /// <param name="livre">Le livre</param>
        private void ShowLivreInfo(Livre livre)
        {
            txtCommandeLivresTitre.Text = livre.Titre;
            txtCommandeLivresAuteur.Text = livre.Auteur;
            txtCommandeLivresIsbn.Text = livre.Isbn;
            txtCommandeLivresCollection.Text = livre.Collection;
            txtCommandeLivresGenre.Text = livre.Genre;
            txtCommandeLivresPublic.Text = livre.Public;
            txtCommandeLivresRayon.Text = livre.Rayon;
            txtCommandeLivresCheminImage.Text = livre.Image;
            string image = livre.Image;
            try
            {
                pictureBox1.Image = Image.FromFile(image);
            }
            catch
            {
                pictureBox1.Image = null;
            }
            UpdateCommandesLivre();
        }

        /// <summary>
        /// Selon le libelle dans la txtBox, affichage des étapes de suivi correspondantes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblEtapeSuivi_TextChanged(object sender, EventArgs e)
        {
            string etapeSuivi = lblEtapeSuivi.Text;
            RemplirCbxCommandeLivreLibelleSuivi(etapeSuivi);
        }

        /// <summary>
        /// Remplissage de la comboBox selon les étapes de suivi et le libelle correspondant
        /// </summary>
        /// <param name="etapeSuivi">Etapes de suivi possibles d'une commande de livre</param>
        private void RemplirCbxCommandeLivreLibelleSuivi(string etapeSuivi)
        {
            cbxCommandeLivresLibelleSuivi.Items.Clear();
            if (etapeSuivi == "livrée")
            {
                cbxCommandeLivresLibelleSuivi.Text = "";
                cbxCommandeLivresLibelleSuivi.Items.Add("réglée");
            }
            else if (etapeSuivi == "en cours")
            {
                cbxCommandeLivresLibelleSuivi.Text = "";
                cbxCommandeLivresLibelleSuivi.Items.Add("relancée");
                cbxCommandeLivresLibelleSuivi.Items.Add("livrée");
            }
            else if (etapeSuivi == "relancée")
            {
                cbxCommandeLivresLibelleSuivi.Text = "";
                cbxCommandeLivresLibelleSuivi.Items.Add("en cours");
                cbxCommandeLivresLibelleSuivi.Items.Add("livrée");
            }
        }

        /// <summary>
        /// Récupération de l'id de suivi d'une commande selon son libelle
        /// </summary>
        /// <param name="libelle">Libelle de l'étape de suivi d'une commande</param>
        /// <returns></returns>
        private string GetIdSuivi(string libelle)
        {
            List<Suivi> lesSuivisCommande = controller.GetAllSuivis();
            foreach (Suivi unSuivi in lesSuivisCommande)
            {
                if (unSuivi.Libelle == libelle)
                {
                    return unSuivi.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Affichage des informations de la commande sélectionnée 
        /// Masque le bouton "Edit étape de suivi" si étape finale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeslivre_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvCommandesLivre.Rows[e.RowIndex];

            string id = row.Cells["Id"].Value.ToString();
            DateTime dateCommande = (DateTime)row.Cells["dateCommande"].Value;
            double montant = double.Parse(row.Cells["Montant"].Value.ToString());
            int nbExemplaire = int.Parse(row.Cells["NbExemplaire"].Value.ToString());
            string libelle = row.Cells["Libelle"].Value.ToString();

            txtCommandeLivreNumero.Text = id;
            txtCommandeLivreNbExemplaires.Text = nbExemplaire.ToString();
            txtCommandeLivreMontant.Text = montant.ToString();
            dtpCommandeLivre.Value = dateCommande;
            lblEtapeSuivi.Text = libelle;

            if (GetIdSuivi(libelle) == "00003")
            {
                cbxCommandeLivresLibelleSuivi.Enabled = false;
                btnReceptionCommandeLivresEditSuivi.Enabled = false;
            }
            else
            {
                cbxCommandeLivresLibelleSuivi.Enabled = true;
                btnReceptionCommandeLivresEditSuivi.Enabled = true;
            }

        }

        /// <summary>
        /// Tri sur les colonnes par ordre inverse de la chronologie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandesLivre_ColumnHeaderMouseClick_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvCommandesLivre.Columns[e.ColumnIndex].HeaderText;
            List<CommandeDocument> sortedList = new List<CommandeDocument>();
            switch (titreColonne)
            {
                case "Date de commande":
                    sortedList = cmdDoc.OrderBy(o => o.DateCommande).Reverse().ToList();
                    break;
                case "Montant":
                    sortedList = cmdDoc.OrderBy(o => o.Montant).ToList();
                    break;
                case "Nombre d'exemplaires":
                    sortedList = cmdDoc.OrderBy(o => o.NbExemplaire).ToList();
                    break;
                case "Suivi":
                    sortedList = cmdDoc.OrderBy(o => o.Libelle).ToList();
                    break;
            }
            RemplirListeCommandesLivres(sortedList);
        }

        /// <summary>
        /// Enregistrement d'une commande de livre dans la base de données
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionCommandeLivreValider_Click(object sender, EventArgs e)
        {
            if (!txtCommandeLivreNumero.Text.Equals("") && !txtCommandeLivreNbExemplaires.Text.Equals("") && !txtCommandeLivreMontant.Text.Equals(""))
            {
                string id = txtCommandeLivreNumero.Text;
                int nbExemplaire = int.Parse(txtCommandeLivreNbExemplaires.Text);
                double montant = double.Parse(txtCommandeLivreMontant.Text);
                DateTime dateCommande = dtpCommandeLivre.Value;
                string idLivreDvd = txtCommandesLivresNumRecherche.Text;
                string idSuivi = lesSuivis[0].Id;

                Commande commande = new Commande(id, dateCommande, montant);

                var idCommandeLivreExistante = controller.GetCommandes(id);
                var idCommandeLivreNonExistante = !idCommandeLivreExistante.Any();

                if (idCommandeLivreNonExistante)
                {
                    if (controller.CreateCommande(commande))
                    {
                        controller.CreateCommandeDocument(id, nbExemplaire, idLivreDvd, idSuivi);
                        MessageBox.Show("La commande " + id + " a bien été enregistrée.", "Information");
                        UpdateCommandesLivre();
                    }
                }
                else
                {
                    MessageBox.Show("Le numéro de la commande existe déjà, veuillez saisir un nouveau numéro.", "Erreur");
                }
            }
            else
            {
                MessageBox.Show("Tous les champs sont obligatoires.", "Information");
            }
        }

        /// <summary>
        /// Modification de l'étape de suivi d'une commande de livre dans la base de données
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEditSuiviCommandeLivre_Click(object sender, EventArgs e)
        {
            string id = txtCommandeLivreNumero.Text;
            int nbExemplaire = int.Parse(txtCommandeLivreNbExemplaires.Text);
            double montant = double.Parse(txtCommandeLivreMontant.Text);
            DateTime dateCommande = dtpCommandeLivre.Value;
            string idLivreDvd = txtCommandesLivresNumRecherche.Text;
            string idSuivi = GetIdSuivi(cbxCommandeLivresLibelleSuivi.Text);

            try
            {
                string libelle = cbxCommandeLivresLibelleSuivi.SelectedItem.ToString();

                CommandeDocument commandedocument = new CommandeDocument(id, dateCommande, montant, nbExemplaire, idLivreDvd, idSuivi, libelle);
                if (MessageBox.Show("Voulez-vous modifier le suivi de la commande " + commandedocument.Id + " en " + libelle + " ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    controller.EditSuiviCommandeDocument(commandedocument.Id, commandedocument.IdSuivi);
                    MessageBox.Show("L'étape de suivi de la commande " + id + " a bien été modifiée.", "Information");
                    UpdateCommandesLivre();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("La nouvelle étape de suivi de la commande doit être sélectionnée.", "Information");
            }
        }

        /// <summary>
        /// Suppression d'une commande dans la base de données
        /// Si elle n'a pas encore été livrée 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivreDelete_Click(object sender, EventArgs e)
        {
            if (dgvCommandesLivre.SelectedRows.Count > 0)
            {
                CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesLivre.List[bdgCommandesLivre.Position];
                if (commandeDocument.Libelle == "en cours" || commandeDocument.Libelle == "relancée")
                {
                    if (MessageBox.Show("Voulez-vous vraiment supprimer la commande " + commandeDocument.Id + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        controller.DeleteCommandeDocument(commandeDocument);
                        UpdateCommandesLivre();
                    }
                }
                else
                {
                    MessageBox.Show("La commande sélectionnée a été livrée, elle ne peut pas être supprimée.", "Information");
                }
            }
            else
            {
                MessageBox.Show("Une ligne doit être sélectionnée.", "Information");
            }
        }

        #endregion

        #region Onglet CommandesDvd
        private readonly BindingSource bdgCommandesDvd = new BindingSource();

        /// <summary>
        /// Ouverture de l'onglet Commandes de dvd :
        /// appel des méthodes pour remplir le datagrid des commandes de dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandesDvd_Enter(object sender, EventArgs e)
        {
            lesDvd = controller.GetAllDvd();
            lesSuivis = controller.GetAllSuivis();
            dtpCommandeDvd.Value = DateTime.Now;
            gbxInfosCommandeDvd.Enabled = false;
            gbxEtapeSuiviDvd.Enabled = false;
        }

        /// <summary>
        /// Masque la groupBox des suivis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gbxInfosCommandeDvd_Enter(object sender, EventArgs e)
        {
            gbxEtapeSuiviDvd.Enabled = false;
        }

        /// <summary>
        /// Affiche la groupBox des suivis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInfosCommandeDvdAnnuler_Click(object sender, EventArgs e)
        {
            gbxEtapeSuiviDvd.Enabled = true;
            gbxInfosCommandeDvd.Enabled = false;
        }

        /// <summary>
        /// Masque la groupBox des informations de commande et le numéro de recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gbxEtapeSuiviDvd_Enter(object sender, EventArgs e)
        {
            gbxInfosCommandeDvd.Enabled = false;
            txtCommandesDvdNumRecherche.Enabled = false;
        }

        /// <summary>
        /// Affiche la groupBox des commandes et le numéro de recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEtapeSuiviAnnulerDvd_Click(object sender, EventArgs e)
        {
            gbxEtapeSuiviDvd.Enabled = false;
            gbxInfosCommandeDvd.Enabled = true;
            txtCommandesDvdNumRecherche.Enabled = true;
        }


        /// <summary>
        /// Remplit la datagrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="lesCommandesDocument">Liste des commandes d'un document</param>
        private void RemplirCommandesDvdListe(List<CommandeDocument> lesCommandesDocument)
        {
            if (lesCommandesDocument != null)
            {
                bdgCommandesDvd.DataSource = lesCommandesDocument;
                dgvCommandesDvd.DataSource = bdgCommandesDvd;
                dgvCommandesDvd.Columns["id"].Visible = false;
                dgvCommandesDvd.Columns["idLivreDvd"].Visible = false;
                dgvCommandesDvd.Columns["idSuivi"].Visible = false;
                dgvCommandesDvd.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvCommandesDvd.Columns["dateCommande"].DisplayIndex = 0;
                dgvCommandesDvd.Columns["montant"].DisplayIndex = 1;
                dgvCommandesDvd.Columns[5].HeaderCell.Value = "Date de commande";
                dgvCommandesDvd.Columns[0].HeaderCell.Value = "Nombre d'exemplaires";
                dgvCommandesDvd.Columns[3].HeaderCell.Value = "Suivi";
            }
            else
            {
                bdgCommandesDvd.DataSource = null;
            }
        }

        /// <summary>
        /// Mise à jour de la liste des commandes de dvd
        /// </summary>
        private void AfficheReceptionCommandesDvd()
        {
            string idDocument = txtCommandesDvdNumRecherche.Text;
            cmdDoc = controller.GetCommandesDocument(idDocument);
            RemplirCommandesDvdListe(cmdDoc);
        }

        /// <summary>
        /// Recherche les commandes concernant le dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandesDvdNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txtCommandesDvdNumRecherche.Text.Equals(""))
            {
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txtCommandesDvdNumRecherche.Text));
                if (dvd != null)
                {
                    AfficheReceptionCommandesDvd();
                    gbxInfosCommandeDvd.Enabled = true;
                    AfficheReceptionCommandesDvdInfos(dvd);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                }
            }
            else
            {
                MessageBox.Show("Le numéro de document est obligatoire", "Information");
            }
        }

        /// <summary>
        /// Affichage des informations du dvd
        /// </summary>
        /// <param name="dvd">Le dvd</param>
        private void AfficheReceptionCommandesDvdInfos(Dvd dvd)
        {
            txtCommandeDvdTitre.Text = dvd.Titre;
            txtCommandeDvdRealisateur.Text = dvd.Realisateur;
            txtCommandeDvdSynopsis.Text = dvd.Synopsis;
            txtCommandeDvdDuree.Text = dvd.Duree.ToString();
            txtCommandeDvdGenre.Text = dvd.Genre;
            txtCommandeDvdPublic.Text = dvd.Public;
            txtCommandeDvdRayon.Text = dvd.Rayon;
            txtCommandeDvdCheminImage.Text = dvd.Image;
            string image = dvd.Image;
            try
            {
                pictureBox2.Image = Image.FromFile(image);
            }
            catch
            {
                pictureBox2.Image = null;
            }
            AfficheReceptionCommandesDvd();
        }

        /// <summary>
        /// Selon le libelle dans la txtBox, affichage des étapes de suivi correspondantes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblSuiviEtapeDvd_TextChanged(object sender, EventArgs e)
        {
            string etapeSuivi = lblEtapeSuiviDvd.Text;
            RemplirCbxCommandeDvdLibelleSuivi(etapeSuivi);
        }

        /// <summary>
        /// Remplissage de la liste de suivi des commandes de dvd
        /// </summary>
        /// <param name="etapeSuivi">Etapes de suivi possibles d'une commande de Dvd</param>
        private void RemplirCbxCommandeDvdLibelleSuivi(string etapeSuivi)
        {
            cbxCommandeDvdLibelleSuivi.Items.Clear();
            if (etapeSuivi == "livrée")
            {
                cbxCommandeDvdLibelleSuivi.Text = "";
                cbxCommandeDvdLibelleSuivi.Items.Add("réglée");
            }
            else if (etapeSuivi == "en cours")
            {
                cbxCommandeDvdLibelleSuivi.Text = "";
                cbxCommandeDvdLibelleSuivi.Items.Add("relancée");
                cbxCommandeDvdLibelleSuivi.Items.Add("livrée");
            }
            else if (etapeSuivi == "relancée")
            {
                cbxCommandeDvdLibelleSuivi.Text = "";
                cbxCommandeDvdLibelleSuivi.Items.Add("en cours");
                cbxCommandeDvdLibelleSuivi.Items.Add("livrée");
            }
        }

        /// <summary>
        /// Affiche les informations de la commande sélectionnée 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeDvd_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvCommandesDvd.Rows[e.RowIndex];

            string id = row.Cells["Id"].Value.ToString();
            DateTime dateCommande = (DateTime)row.Cells["dateCommande"].Value;
            double montant = double.Parse(row.Cells["Montant"].Value.ToString());
            int nbExemplaire = int.Parse(row.Cells["NbExemplaire"].Value.ToString());
            string libelle = row.Cells["Libelle"].Value.ToString();

            txtCommandeDvdNumero.Text = id;
            txtCommandeDvdNbExemplaires.Text = nbExemplaire.ToString();
            txtCommandeDvdMontant.Text = montant.ToString();
            dtpCommandeDvd.Value = dateCommande;

            lblEtapeSuiviDvd.Text = libelle;
            if (GetIdSuivi(libelle) == "00003")
            {
                cbxCommandeDvdLibelleSuivi.Enabled = false;
                btnReceptionCommandeDvdEditSuivi.Enabled = false;
            }
            else
            {
                cbxCommandeDvdLibelleSuivi.Enabled = true;
                btnReceptionCommandeDvdEditSuivi.Enabled = true;
            }
        }

        /// <summary>
        /// Tri sur les colonnes par ordre inverse de la chronologie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandesDvd_ColumnHeaderMouseClick_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvCommandesDvd.Columns[e.ColumnIndex].HeaderText;
            List<CommandeDocument> sortedList = new List<CommandeDocument>();
            switch (titreColonne)
            {
                case "Date de commande":
                    sortedList = cmdDoc.OrderBy(o => o.DateCommande).Reverse().ToList();
                    break;
                case "Montant":
                    sortedList = cmdDoc.OrderBy(o => o.Montant).ToList();
                    break;
                case "Nombre d'exemplaires":
                    sortedList = cmdDoc.OrderBy(o => o.NbExemplaire).ToList();
                    break;
                case "Suivi":
                    sortedList = cmdDoc.OrderBy(o => o.Libelle).ToList();
                    break;
            }
            RemplirCommandesDvdListe(sortedList);
        }

        /// <summary>
        /// Enregistrement d'une nouvelle commande de dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionCommandeDvdValider_Click(object sender, EventArgs e)
        {
            if (!txtCommandeDvdNumero.Text.Equals("") && !txtCommandeDvdNbExemplaires.Text.Equals("") && !txtCommandeDvdMontant.Text.Equals(""))
            {
                string idLivreDvd = txtCommandesDvdNumRecherche.Text;
                string idSuivi = lesSuivis[0].Id;
                string id = txtCommandeDvdNumero.Text;
                int nbExemplaire = int.Parse(txtCommandeDvdNbExemplaires.Text);
                double montant = double.Parse(txtCommandeDvdMontant.Text);
                DateTime dateCommande = dtpCommandeDvd.Value;

                Commande commande = new Commande(id, dateCommande, montant);

                var idCommandeExistante = controller.GetCommandes(id);
                var idCommandeNonExistante = !idCommandeExistante.Any();

                if (idCommandeNonExistante)
                {
                    if (controller.CreateCommande(commande))
                    {
                        controller.CreateCommandeDocument(id, nbExemplaire, idLivreDvd, idSuivi);
                        MessageBox.Show("La commande " + id + " a bien été enregistrée.", "Information");
                        AfficheReceptionCommandesDvd();
                    }
                }
                else
                {
                    MessageBox.Show("Le numéro de la commande existe déjà, veuillez saisir un nouveau numéro.", "Erreur");
                }
            }
            else
            {
                MessageBox.Show("Tous les champs sont obligatoires.", "Information");
            }
        }

        /// <summary>
        /// Modification de l'étape de suivi d'une commande de dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionCommandeDvdEditSuivi_Click(object sender, EventArgs e)
        {
            try
            {
                string idLivreDvd = txtCommandesDvdNumRecherche.Text;
                string idSuivi = GetIdSuivi(cbxCommandeDvdLibelleSuivi.Text);
                string libelle = cbxCommandeDvdLibelleSuivi.SelectedItem.ToString();
                string id = txtCommandeDvdNumero.Text;
                int nbExemplaire = int.Parse(txtCommandeDvdNbExemplaires.Text);
                double montant = double.Parse(txtCommandeDvdMontant.Text);
                DateTime dateCommande = dtpCommandeDvd.Value;

                CommandeDocument commandedocument = new CommandeDocument(id, dateCommande, montant, nbExemplaire, idLivreDvd, idSuivi, libelle);
                if (MessageBox.Show("Voulez-vous modifier le suivi de la commande " + commandedocument.Id + " en " + libelle + " ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    controller.EditSuiviCommandeDocument(commandedocument.Id, commandedocument.IdSuivi);
                    AfficheReceptionCommandesDvd();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("La nouvelle étape de suivi de la commande doit être sélectionnée.", "Information");
            }
        }

        /// <summary>
        /// Suppression d'une commande de dvd si elle n'a pas encore été livrée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdDelete_Click(object sender, EventArgs e)
        {
            if (dgvCommandesDvd.SelectedRows.Count > 0)
            {
                CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesDvd.List[bdgCommandesDvd.Position];
                if (commandeDocument.Libelle == "en cours" || commandeDocument.Libelle == "relancée")
                {
                    if (MessageBox.Show("Voulez-vous vraiment supprimer la commande " + commandeDocument.Id + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        controller.DeleteCommandeDocument(commandeDocument);
                        AfficheReceptionCommandesDvd();
                    }
                }
                else
                {
                    MessageBox.Show("La commande sélectionnée a été livrée elle ne peut pas être supprimée.", "Information");
                }
            }
            else
            {
                MessageBox.Show("Une ligne doit être sélectionnée.", "Information");
            }
        }


        #endregion

        #region Onglet CommandesRevues

        private readonly BindingSource bdgAbonnementsRevue = new BindingSource();
        private List<Abonnement> lesAbonnementsRevue = new List<Abonnement>();

        /// <summary>
        /// Ouverture de l'onglet Commandes de revues :
        /// appel des méthodes pour remplir le datagrid des abonnements d'une revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandesRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            gbxInfosCommandeRevue.Enabled = false;
            dtpCommandeDvd.Value = DateTime.Now;
        }

        /// <summary>
        /// Remplit la datagrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="lesAbonnementsRevue">Liste des abonnements d'une revue</param>
        private void RemplirAbonnementsRevueListe(List<Abonnement> lesAbonnementsRevue)
        {
            if (lesAbonnementsRevue != null)
            {
                bdgAbonnementsRevue.DataSource = lesAbonnementsRevue;
                dgvAbonnementsRevue.DataSource = bdgAbonnementsRevue;
                dgvAbonnementsRevue.Columns["id"].Visible = false;
                dgvAbonnementsRevue.Columns["idRevue"].Visible = false;
                dgvAbonnementsRevue.Columns["titre"].Visible = false;
                dgvAbonnementsRevue.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvAbonnementsRevue.Columns["dateCommande"].DisplayIndex = 0;
                dgvAbonnementsRevue.Columns["montant"].DisplayIndex = 1;
                dgvAbonnementsRevue.Columns[4].HeaderCell.Value = "Date de commande";
                dgvAbonnementsRevue.Columns[0].HeaderCell.Value = "Date de fin d'abonnement";
            }
            else
            {
                bdgAbonnementsRevue.DataSource = null;
            }
        }

        /// <summary>
        /// Affiche la liste des abonnements d'une revue
        /// </summary>
        private void AfficheReceptionAbonnementsRevue()
        {
            string idDocument = txtCommandesRevueNumRecherche.Text;
            lesAbonnementsRevue = controller.GetAbonnementRevue(idDocument);
            RemplirAbonnementsRevueListe(lesAbonnementsRevue);
        }

        /// <summary>
        /// Recherche d'une revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandesRevueNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txtCommandesRevueNumRecherche.Text.Equals(""))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(txtCommandesRevueNumRecherche.Text));
                if (revue != null)
                {
                    AfficheReceptionAbonnementsRevue();
                    gbxInfosCommandeRevue.Enabled = true;
                    AfficheReceptionAbonnementsRevueInfos(revue);
                }
                else
                {
                    MessageBox.Show("Ce numéro de revue n'existe pas.");
                }
            }
            else
            {
                MessageBox.Show("Le numéro de revue est obligatoire.");
            }
        }

        /// <summary>
        /// Affichage des informations d'une revue
        /// </summary>
        /// <param name="revue">La revue</param>
        private void AfficheReceptionAbonnementsRevueInfos(Revue revue)
        {
            txtCommandeRevueTitre.Text = revue.Titre;
            txtCommandeRevuePeriodicite.Text = revue.Periodicite;
            txtCommandeRevueDelai.Text = revue.DelaiMiseADispo.ToString();
            txtCommandeRevueGenre.Text = revue.Genre;
            txtCommandeRevuePublic.Text = revue.Public;
            txtCommandeRevueRayon.Text = revue.Rayon;
            txtCommandeRevueCheminImage.Text = revue.Image;
            string image = revue.Image;
            try
            {
                pictureBox4.Image = Image.FromFile(image);
            }
            catch
            {
                pictureBox4.Image = null;
            }
            AfficheReceptionAbonnementsRevue();
        }

        /// <summary>
        /// Affichage des informations de l'abonnement sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAbonnementsRevue_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvAbonnementsRevue.Rows[e.RowIndex];
            string id = row.Cells["Id"].Value.ToString();
            DateTime dateCommande = (DateTime)row.Cells["dateCommande"].Value;
            double montant = double.Parse(row.Cells["Montant"].Value.ToString());
            DateTime dateFinAbonnement = (DateTime)row.Cells["DateFinAbonnement"].Value;

            txtCommandeRevueNumero.Text = id;
            txtCommandeRevueMontant.Text = montant.ToString();
            dtpCommandeRevue.Value = dateCommande;
            dtpCommandeRevueAbonnementFin.Value = dateFinAbonnement;
        }

        /// <summary>
        /// Tri sur les colonnes par ordre inverse de la chronologie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAbonnementsRevue_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvAbonnementsRevue.Columns[e.ColumnIndex].HeaderText;
            List<Abonnement> sortedList = new List<Abonnement>();
            switch (titreColonne)
            {
                case "Date de commande":
                    sortedList = lesAbonnementsRevue.OrderBy(o => o.DateCommande).Reverse().ToList();
                    break;
                case "Montant":
                    sortedList = lesAbonnementsRevue.OrderBy(o => o.Montant).ToList();
                    break;
                case "Date de fin d'abonnement":
                    sortedList = lesAbonnementsRevue.OrderBy(o => o.DateFinAbonnement).Reverse().ToList();
                    break;
            }
            RemplirAbonnementsRevueListe(sortedList);
        }

        /// <summary>
        /// Enregistrement d'un abonnement de revue dans la base de données
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionAbonnementRevueValider_Click(object sender, EventArgs e)
        {
            if (!txtCommandeRevueNumero.Text.Equals("") && !txtCommandeRevueMontant.Text.Equals(""))
            {
                string idRevue = txtCommandesRevueNumRecherche.Text;
                string id = txtCommandeRevueNumero.Text;
                double montant = double.Parse(txtCommandeRevueMontant.Text);
                DateTime dateCommande = dtpCommandeRevue.Value;
                DateTime dateFinAbonnement = dtpCommandeRevueAbonnementFin.Value;

                Commande commande = new Commande(id, dateCommande, montant);

                var idCommandeRevueExistante = controller.GetCommandes(id);
                var idCommandeRevueNonExistante = !idCommandeRevueExistante.Any();

                if (idCommandeRevueNonExistante)
                {
                    if (controller.CreateCommande(commande))
                    {
                        controller.CreateAbonnementRevue(id, dateFinAbonnement, idRevue);
                        MessageBox.Show("La commande " + id + " a bien été enregistrée.", "Information");
                        AfficheReceptionAbonnementsRevue();
                    }
                }
                else
                {
                    MessageBox.Show("Le numéro de la commande existe déjà, veuillez saisir un nouveau numéro.", "Erreur");
                }
            }
            else
            {
                MessageBox.Show("Tous les champs sont obligatoires", "Information");
            }
        }

        /// <summary>
        /// Retourne vrai si la date de parution est entre les 2 autres dates
        /// </summary>
        /// <param name="dateCommande">Date de la commande d'un abonnement à une revue</param>
        /// <param name="dateFinAbonnement">Date de fin d'abonnement à une revue</param>
        /// <param name="dateParution">Date de parution d'un exemplaire</param>
        /// <returns></returns>
        public bool ParutionDansAbonnement(DateTime dateCommande, DateTime dateFinAbonnement, DateTime dateParution)
        {
            return (DateTime.Compare(dateCommande, dateParution) < 0 && DateTime.Compare(dateParution, dateFinAbonnement) < 0);
        }

        /// <summary>
        /// Vérifie si aucun exemplaire n'est rattaché à un abonnement de revue
        /// </summary>
        /// <param name="abonnement">L'abonnement</param>
        /// <returns></returns>
        public bool VerificationExemplaire(Abonnement abonnement)
        {
            List<Exemplaire> lesExemplairesAbonnement = controller.GetExemplairesRevue(abonnement.IdRevue);
            bool datedeparution = false;
            foreach (Exemplaire exemplaire in lesExemplairesAbonnement.Where(exemplaires => ParutionDansAbonnement(abonnement.DateCommande, abonnement.DateFinAbonnement, exemplaires.DateAchat)))
            {
                datedeparution = true;

            }
            return !datedeparution;
        }

        /// <summary>
        /// Suppression d'un abonnement de revue dans la base de données
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbonnementRevueDelete_Click(object sender, EventArgs e)
        {
            if (dgvAbonnementsRevue.SelectedRows.Count > 0)
            {
                Abonnement abonnement = (Abonnement)bdgAbonnementsRevue.Current;
                if (MessageBox.Show("Souhaitez-vous confirmer la suppression de l'abonnement " + abonnement.Id + " ?", "Confirmation de la suppression", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {

                    if (VerificationExemplaire(abonnement))
                    {
                        if (controller.DeleteAbonnementRevue(abonnement))
                        {
                            AfficheReceptionAbonnementsRevue();
                        }
                        else
                        {
                            MessageBox.Show("Une erreur s'est produite.", "Erreur");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cet abonnement contient un ou plusieurs exemplaires, il ne peut donc pas être supprimé.", "Information");
                    }
                }
            }
            else
            {
                MessageBox.Show("Une ligne doit être sélectionnée.", "Information");
            }
        }


        #endregion
    }
}
