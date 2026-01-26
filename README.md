# 🚗 Application de Gestion de Maintenance d’un Parc Automobile

Application desktop permettant de gérer la maintenance d’un parc automobile : suivi des véhicules, entretiens, alertes automatiques et statistiques de coûts.

Projet réalisé dans le cadre d’un **BTS SIO / SN** par un groupe de deux étudiants, en **C# (.NET) avec interface WPF**, selon l’architecture **MVVM**.

---

## 📌 Contexte du projet

Une entreprise dispose d’un parc automobile utilisé quotidiennement.  
Elle souhaite un **logiciel interne** permettant :

- d’assurer le suivi des entretiens,
- d’éviter les retards de maintenance,
- de maîtriser les coûts,
- de sécuriser l’accès selon les rôles des utilisateurs.

L’application doit être **simple d’utilisation**, **sécurisée** et **adaptée aux différents profils** (patron, mécanicien, secrétaire).

---

## 🎯 Objectifs

L’application permet de :

- gérer un parc de véhicules,
- enregistrer les opérations d’entretien et de réparation,
- consulter l’historique de maintenance,
- générer des alertes automatiques,
- produire des statistiques détaillées,
- sécuriser l’accès via une authentification avec rôles.

---

## 👥 Gestion des utilisateurs et des rôles

### Authentification
- Page de connexion sécurisée
- Mots de passe **hashés (SHA256)**
- Vérification via base de données

### Rôles et permissions

| Rôle        | Droits |
|------------|--------|
| **Patron** | Tous les droits : véhicules, entretiens, coûts, statistiques, utilisateurs |
| **Mécanicien** | Consultation des véhicules + ajout d’entretiens |
| **Secrétaire** | Gestion administrative, consultation de l’historique (restrictions sur coûts et suppressions) |

---

## 🚘 Gestion du parc automobile

Fonctionnalités :
- Ajouter, modifier, supprimer un véhicule
- Consulter la liste des véhicules
- Rechercher / filtrer (immatriculation, modèle, statut)
- Accéder à la fiche détaillée d’un véhicule

### Informations d’un véhicule
- Immatriculation  
- Marque et modèle  
- Année  
- Kilométrage actuel  
- Date du dernier entretien  
- Statut (en service, hors service, en réparation)  
- Type de carburant  

---

## 🔧 Gestion des entretiens et réparations

Pour chaque véhicule :
- Ajout d’une intervention (vidange, pneus, freins, CT…)
- Informations : garage, date, durée, coût
- Mise à jour automatique du kilométrage et des dates
- Consultation de l’historique complet

### Restrictions par rôle
- **Mécanicien** : ajout uniquement
- **Secrétaire** : consultation, pas de modification des coûts
- **Patron** : accès complet

---

## ⏰ Alertes de maintenance

Alertes automatiques basées sur :
- le kilométrage (ex : vidange tous les 15 000 km),
- la date (ex : contrôle technique tous les 2 ans),
- l’historique des entretiens,
- des seuils paramétrables.

Les alertes sont visibles :
- sur le tableau de bord,
- sur la fiche du véhicule concerné.

---

## 📊 Statistiques

Module réservé au **rôle Patron** :

- coût total par véhicule,
- répartition des types d’interventions,
- coûts mensuels et annuels,
- top 5 des véhicules les plus coûteux,
- nombre d’interventions par type,
- graphiques d’évolution.

---

## 📤 Exports

- Export **PDF** du rapport complet d’un véhicule
- Export **CSV** de l’historique des entretiens

---

## 🛠️ Technologies utilisées

- **Langage** : C# (.NET)
- **Interface** : WPF
- **Architecture** : MVVM
- **Base de données** : SQLite
- **Accès aux données** : LINQ / Entity Framework
- **Outils** : Visual Studio

---

## 🔐 Sécurité

- Mots de passe hashés
- Requêtes paramétrées
- Gestion stricte des droits par rôle
- Aucune donnée sensible stockée en clair

---

## 🎨 Ergonomie

- Interface intuitive et claire
- Thème clair (option sombre possible)
- Écrans épurés
- Boutons explicites
- Dialogues de confirmation pour les actions critiques

---

## 📦 Livrables

- Cahier des charges
- Diagrammes UML (cas d’utilisation + classes)
- Schéma de base de données
- Maquettes WPF
- Code source
- Exécutable final
- Dossier technique et utilisateur
- Présentation orale

---

## ✅ Critères de réussite

Le projet est validé si :
- toutes les fonctionnalités sont implémentées,
- les rôles fonctionnent correctement,
- la base de données est cohérente,
- l’interface est stable et ergonomique,
- les statistiques sont exploitables,
- la présentation est professionnelle.
