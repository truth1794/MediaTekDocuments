using System;
using MediaTekDocuments.controller;
using MediaTekDocuments.model;
using System.Windows.Forms;

namespace MediaTekDocuments.view
{
    /// <summary>
    /// Classe d'affichage de la fenêtre d'authentification
    /// </summary>
    public partial class FrmAuthentification : Form
    {
        private readonly FrmAuthentificationController controller;

        /// <summary>
        /// Constructeur : création du contrôleur lié à ce formulaire
        /// </summary>
        public FrmAuthentification()
        {
            InitializeComponent();
            this.controller = new FrmAuthentificationController();
        }

        /// <summary>
        /// Connexion à l'application grâce aux identifiants saisis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnexion_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text;
            string pwd = txtPwd.Text;

            if (!txtLogin.Text.Equals("") && !txtPwd.Text.Equals(""))
            {
                Service service = controller.GetUser(login, pwd);
                //Service service = new Service("0", "administrateur");

                if (service == null)
                {
                    MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect", "Alerte");
                    txtLogin.Text = "";
                    txtPwd.Text = "";
                }
                else if(service.Libelle == "culture")
                {
                    MessageBox.Show("Les droits de ce compte sont insuffisants pour accéder à cette application !", "Alerte");
                    Application.Exit();
                }
                else
                {
                    //MessageBox.Show("Vous êtes connecté", "Information");
                    FrmMediatek frmMediatek = new FrmMediatek(service);
                    this.Hide();
                    frmMediatek.ShowDialog();
                    
                }
            }
            else
            {
                MessageBox.Show("Tous les champs sont obligatoires");
            }
        }
    }
}
