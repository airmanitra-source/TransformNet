// --- 1. CONFIGURATION ---
using MachineLearning.ApiService.Models;

int dimension = 8;      // Taille des vecteurs de sens
int nbHeads = 2;        // Nombre de têtes d'attention
int hiddenSize = 32;    // Taille de la couche cachée du FFN
string corpus = "le chat mange la souris . le chat dort . la souris court . le chat joue . la souris mange . le chat regarde la souris . la souris fuit le chat . le chat dort près de la souris .";

// --- 2. INITIALISATION ---
var vocab = new Vocabulaire();
vocab.Construire(corpus);
int tailleVocab = vocab.Compter();

var embedding = new EmbeddingLayer(tailleVocab, dimension);
var positionalEncoding = new PositionalEncoding(dimension);
var block = new MultiHeadTransformer(dimension, nbHeads, hiddenSize, tailleVocab);
var generateur = new GenerateurTexte();
var gestionnaire = new GestionnairePoids();

// --- 3. ENTRAÎNEMENT ---
Console.WriteLine("Début de l'entrainement...");
var entraineur = new EntraineurTransformer(block, embedding, positionalEncoding, vocab);

// Extraire les mots et cibles du corpus
var motsDuCorpus = corpus.ToLower()
    .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\t' },
           StringSplitOptions.RemoveEmptyEntries);
double[] cibles = vocab.ExtraireCibles(corpus);
string[] motSequence = motsDuCorpus.Take(cibles.Length).ToArray();

entraineur.Train(motSequence, cibles, nbEpochs: 5000);
Console.WriteLine("Entraînement terminé.\n");

// --- 4. SAUVEGARDE ---
gestionnaire.Sauvegarder("mon_modele.json", block, embedding, vocab);
Console.WriteLine("Modèle sauvegardé dans 'mon_modele.json'.\n");

// --- 5. GÉNÉRATION (INFERENCE) ---
string debutPhrase = "le chat";
Console.Write($"Début : {debutPhrase} ");

for (int i = 0; i < 5; i++)
{
    // Tokeniser la phrase actuelle depuis les embeddings appris
    double[][] seq = vocab.Tokeniser(debutPhrase, embedding);
    seq= positionalEncoding.GetPositionalEncoding(seq);

    // Passer le dernier token avec toute la séquence comme contexte
    int pos = seq.Length - 1;
    double[] sortie = block.Executer(seq[pos], seq, pos);
    double[] scores = block.ObtenirScoresSortie();

    // Choisir le prochain mot (température modérée)
    int nextId = generateur.ChoisirIndex(scores, temperature: 0.8);
    string motPredi = vocab.GetMot(nextId);

    Console.Write(motPredi + " ");
    debutPhrase += " " + motPredi;
}

Console.WriteLine("\n\nFin de la démonstration.");