using System;
using System.IO;

// Programme principal du jeu
class Program
{
    // Constantes du programme
    private const string FICHIER_MOTS = "Mots.txt";

    static void Main(string[] args)
    {
        // Variables principales
        Pendu jeu = new Pendu();
        string cheminFichier = Path.Combine(AppContext.BaseDirectory, FICHIER_MOTS);
        string saisie = "";
        string message = "";
        int partiesGagnees = 0;
        int partiesPerdues = 0;

        // Chargement du fichier de mots
        try
        {
            jeu.ChargerFichierMots(cheminFichier);
        }
        catch
        {
            Console.WriteLine("Impossible de charger le fichier Mots.txt.");
            Console.WriteLine("Vérifiez que le fichier est dans le projet.");
            Console.ReadKey();
            return;
        }

        // Boucle pour commencer une nouvelle partie
        while (true)
        {
            jeu.ChoisirMot();
            message = "";

            // Boucle qui continue tant que la partie n'est pas terminée
            while (!jeu.Gagne && !jeu.Perdu)
            {
                Console.Clear();
                AfficherInterface(jeu, partiesGagnees, partiesPerdues);

                if (message != "")
                {
                    Console.WriteLine(message);
                }

                Console.WriteLine();
                Console.Write("Entrez une lettre ou le mot : ");
                saisie = Console.ReadLine() ?? "";
                saisie = saisie.Trim();

                // Commande pour quitter le programme
                if (saisie == "QUITTER")
                {
                    return;
                }

                // Une seule lettre appelle ValiderLettre
                if (saisie.Length == 1)
                {
                    message = jeu.ValiderLettre(saisie[0]);
                }
                // Plusieurs lettres appellent ValiderMot
                else if (saisie.Length > 1)
                {
                    message = jeu.ValiderMot(saisie);
                }
                else
                {
                    message = "Veuillez entrer une lettre ou un mot.";
                }
            }

            // Mise à jour du nombre de parties gagnées ou perdues
            if (jeu.Gagne)
            {
                partiesGagnees++;
            }
            else
            {
                partiesPerdues++;
            }

            // Affichage du résultat de la partie
            Console.Clear();
            AfficherInterface(jeu, partiesGagnees, partiesPerdues);
            Console.WriteLine();
            Console.WriteLine(message);

            if (jeu.Gagne)
            {
                Console.WriteLine("Bravo! Vous avez gagné!");
            }
            else
            {
                Console.WriteLine("Vous avez perdu!");
            }

            Console.WriteLine("Le mot était : " + jeu.MotChoisi);
            Console.WriteLine();
            Console.Write("Appuyez sur Entrée pour rejouer ou tapez QUITTER : ");
            saisie = Console.ReadLine() ?? "";

            if (saisie.Trim() == "QUITTER")
            {
                break;
            }
        }
    }

    // Affiche les informations principales du jeu
    static void AfficherInterface(Pendu jeu, int partiesGagnees, int partiesPerdues)
    {
        // Variables pour l'affichage
        string mot = jeu.MotActuel;
        string lettresRestantes = CreerLettresRestantes(jeu);
        string lettresDemandees = jeu.LettresDemandees;

        Console.WriteLine("Jeu du pendu :  Gagnées : " + partiesGagnees + "  Perdues : " + partiesPerdues);
        Console.WriteLine("---------------------------------------------");

        // Affichage du bonhomme pendu
        AfficherPendu(jeu.PartiesCorps);

        // Affichage du mot à trouver
        Console.Write("Mot à trouver : ");
        for (int i = 0; i < mot.Length; i++)
        {
            Console.Write(mot[i] + " ");
        }
        Console.WriteLine();

        // Affichage des lettres restantes et demandées
        Console.WriteLine("Lettres restantes : " + lettresRestantes);

        if (lettresDemandees == "")
        {
            lettresDemandees = "-";
        }
        Console.WriteLine("Lettres demandées : " + lettresDemandees);
    }

    // Affiche le pendu selon le nombre de mauvaises réponses
    static void AfficherPendu(int partiesCorps)
    {
        // Variables qui représentent les parties du corps
        string tete = "";
        string corps = " ";
        string brasGauche = " ";
        string brasDroit = " ";
        string jambeGauche = " ";
        string jambeDroite = " ";

        // Ajout des parties du corps une à la fois
        if (partiesCorps >= 1) tete = "O";
        if (partiesCorps >= 2) corps = "|";
        if (partiesCorps >= 3) brasGauche = "/";
        if (partiesCorps >= 4) brasDroit = "\\";
        if (partiesCorps >= 5) jambeGauche = "/";
        if (partiesCorps >= 6) jambeDroite = "\\";

        // Affichage de la structure et du bonhomme
        Console.WriteLine("  +---+");
        Console.WriteLine("  |   |");
        Console.WriteLine("  |   " + tete);
        Console.WriteLine("  |  " + brasGauche + corps + brasDroit);
        Console.WriteLine("  |  " + jambeGauche + " " + jambeDroite);
        Console.WriteLine(" _|_");
    }

    // Trouve les lettres de l'alphabet qui n'ont pas encore été demandées
    static string CreerLettresRestantes(Pendu jeu)
    {
        // Variables pour la liste des lettres
        string alphabet = "abcdefghijklmnopqrstuvwxyz";
        string lettresRestantes = "";

        for (int i = 0; i < alphabet.Length; i++)
        {
            if (!jeu.LettresDemandees.Contains(alphabet[i]))
            {
                lettresRestantes += alphabet[i];
            }
        }

        return lettresRestantes;
    }
}

// Classe qui contient les données et les méthodes du jeu
class Pendu
{
    // Constantes et variables de la classe
    private const int MAX_MOTS = 100;
    private const int MAX_PARTIES_CORPS = 6;
    private string[] listeMots = new string[MAX_MOTS];
    private int nombreMots = 0;
    private string motChoisi = "";
    private char[] motActuel = new char[0];
    private string lettresDemandees = "";
    private static Random hasard = new Random();

    // Propriétés utilisées par le programme principal
    public int PartiesCorps { get; private set; }
    public bool Gagne { get; private set; }
    public bool Perdu { get; private set; }
    public string MotChoisi { get { return motChoisi; } }
    public string MotActuel { get { return new string(motActuel); } }
    public string LettresDemandees { get { return lettresDemandees; } }

    // Charge un maximum de 100 mots à partir du fichier texte
    public void ChargerFichierMots(string nomFichier)
    {
        // Variables utilisées pour lire le fichier
        string[] lignes = File.ReadAllLines(nomFichier);

        // Préparation du tableau de 100 mots
        listeMots = new string[MAX_MOTS];
        nombreMots = 0;

        // Copie des mots dans le tableau, sans dépasser 100
        for (int i = 0; i < lignes.Length && nombreMots < MAX_MOTS; i++)
        {
            if (!string.IsNullOrWhiteSpace(lignes[i]))
            {
                listeMots[nombreMots] = lignes[i].Trim().ToLowerInvariant();
                nombreMots++;
            }
        }

        // Vérification du fichier
        if (nombreMots == 0)
        {
            throw new Exception("Le fichier ne contient aucun mot.");
        }
    }

    // Choisit un mot au hasard et prépare une nouvelle partie
    public void ChoisirMot()
    {
        // Variables pour préparer la partie
        motChoisi = listeMots[hasard.Next(nombreMots)];
        motActuel = new char[motChoisi.Length];
        lettresDemandees = "";
        PartiesCorps = 0;
        Gagne = false;
        Perdu = false;

        // Cache les lettres du mot avec des tirets bas
        for (int i = 0; i < motChoisi.Length; i++)
        {
            if (char.IsLetter(motChoisi[i]))
            {
                motActuel[i] = '_';
            }
            else
            {
                motActuel[i] = motChoisi[i];
            }
        }
    }

    // Vérifie une lettre et l'affiche si elle est dans le mot
    public string ValiderLettre(char lettre)
    {
        // Variables pour vérifier la lettre
        char lettreNormale = NormaliserLettre(lettre);
        bool trouve = false;

        if (!char.IsLetter(lettre))
        {
            return "Veuillez entrer une lettre valide.";
        }

        // Empêche de demander deux fois la même lettre
        if (lettresDemandees.Contains(lettreNormale))
        {
            return "Vous avez déjà demandé cette lettre.";
        }

        lettresDemandees += lettreNormale;

        // Vérifie toutes les positions du mot
        for (int i = 0; i < motChoisi.Length; i++)
        {
            if (NormaliserLettre(motChoisi[i]) == lettreNormale)
            {
                motActuel[i] = motChoisi[i];
                trouve = true;
            }
        }

        // Une mauvaise lettre ajoute une partie du corps
        if (!trouve)
        {
            PartiesCorps++;
        }

        // Vérifie si la partie est gagnée ou perdue
        VerifierFinPartie();

        if (trouve)
        {
            return "Bonne lettre!";
        }
        return "Mauvaise lettre!";
    }

    // Vérifie si le mot complet entré par le joueur est exact
    public string ValiderMot(string motSaisi)
    {
        // Variable pour comparer les deux mots
        string motNormalise = NormaliserTexte(motSaisi.Trim());
        string motChoisiNormalise = NormaliserTexte(motChoisi);

        if (motNormalise == motChoisiNormalise)
        {
            motActuel = motChoisi.ToCharArray();
            Gagne = true;
            return "Bonne réponse!";
        }

        // Un mauvais mot ajoute une partie du corps
        PartiesCorps++;
        VerifierFinPartie();
        return "Mauvais mot!";
    }

    // Vérifie si toutes les lettres sont trouvées ou si le pendu est complet
    private void VerifierFinPartie()
    {
        // Vérification de la victoire
        if (new string(motActuel) == motChoisi)
        {
            Gagne = true;
        }

        // Vérification de la défaite
        if (PartiesCorps >= MAX_PARTIES_CORPS)
        {
            Perdu = true;
        }
    }

    // Convertit les lettres accentuées en lettres simples
    private char NormaliserLettre(char lettre)
    {
        // Variables pour normaliser la lettre
        char caractere = char.ToLowerInvariant(lettre);

        switch (caractere)
        {
            case 'é':
            case 'è':
            case 'ê':
            case 'ë': return 'e';

            case 'à':
            case 'â':
            case 'ä': return 'a';

            case 'ü':
            case 'û':
            case 'ù': return 'u';

            case 'ö':
            case 'ô':
            case 'ò': return 'o';

            case 'ï':
            case 'ì':
            case 'î': return 'i';

            case 'ç': return 'c';

            default: return caractere;
        }
    }

    // Normalise un mot complet pour comparer les accents sans distinction
    private string NormaliserTexte(string texte)
    {
        // Variable qui contiendra le mot normalisé
        string resultat = "";

        for (int i = 0; i < texte.Length; i++)
        {
            resultat += NormaliserLettre(texte[i]);
        }

        return resultat;
    }
}
