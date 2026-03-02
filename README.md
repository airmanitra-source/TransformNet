# Usage of transformNet

`
int dimension = 8;      // Taille des vecteurs de sens
int nbHeads = 2;        // Nombre de têtes d'attention
int hiddenSize = 32;    // Taille de la couche cachée du FFN
string corpus = @"Le chat mange la souris . 
                  Le chat dort. 
                  La souris court.
                  Le chat court après la souris.
                  Le chat joue avec la souris. 
                  La souris mange quand le chat n'est pas là.
                  Le chat regarde la souris.
                  Regarde la souris. 
                  La souris fuit le chat.
                  Le chat et la souris sort de sa tanière.
                  Chat et chien peuvent jouer ensemble mais pas avec la souris.
                  Le chien ne peut pas attraper la souris.";

// --- 2. INITIALISATION ---
`
var vocab = new Vocabulaire(corpus);
int tailleVocab = vocab.GetWordsCount();
var embedding = new EmbeddingLayer(tailleVocab, dimension);
var positionalEncoding = new PositionalEncoding(dimension);
var transformer = new MultiHeadTransformer(dimension, nbHeads, hiddenSize, tailleVocab);
var gestionnaire = new WeightManager();

// --- 3. ENTRAÎNEMENT ---
// Extraire les mots et cibles du corpus
var motsDuCorpus = corpus.ToLower().Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
double[] cibles = vocab.GetTarget(corpus);
string[] motSequence = motsDuCorpus.Take(cibles.Length).ToArray();
transformer.Train(motSequence, cibles, nbEpochs: 2500);
// --- 4. SAUVEGARDE ---
gestionnaire.Save("mon_modele.json", transformer, embedding, vocab);
// --- 5. GÉNÉRATION (INFERENCE) ---
string[] phrasesDebut = new[] { "le chat", "la souris", "le chat et la souris", "le chien" };
transformer.Predict(phrasesDebut, nbMotsAGenerer: 2, temperature: 0.8);
`
