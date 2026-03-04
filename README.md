# Application de Gestion de Maintenance d’un Parc Automobile

Application desktop permettant de gérer la maintenance d’un parc automobile : suivi des véhicules, entretiens, alertes automatiques et statistiques de coûts.

Projet réalisé dans le cadre d’un **BTS SIO / SN** par un **groupe de trois étudiants**, en **C# (.NET) avec interface WPF**, selon l’architecture **MVVM**.

---

## Contexte du projet

Une entreprise dispose d’un parc automobile utilisé quotidiennement.  
Elle souhaite un logiciel interne permettant :

- d’assurer le suivi des entretiens
- d’éviter les retards de maintenance
- de maîtriser les coûts
- de sécuriser l’accès selon les rôles des utilisateurs

L’application doit être :
- simple d’utilisation
- sécurisée
- adaptée aux différents profils (patron, mécanicien, secrétaire)

---

## Objectifs

L’application permet de :

- gérer un parc de véhicules
- enregistrer les opérations d’entretien et de réparation
- consulter l’historique de maintenance
- générer des alertes automatiques
- produire des statistiques détaillées
- sécuriser l’accès via une authentification avec rôles

---

## Gestion des utilisateurs et des rôles

### Authentification

- page de connexion sécurisée  
- mots de passe hashés (SHA256)  
- vérification via base de données  

### Rôles et permissions

| Rôle | Droits |
|------|--------|
| Patron | Accès complet : véhicules, entretiens, coûts, statistiques, utilisateurs |
| Mécanicien | Consultation des véhicules et ajout d’entretiens |
| Secrétaire | Gestion administrative et consultation de l’historique (restrictions sur coûts et suppressions) |

---

## Gestion du parc automobile

Fonctionnalités :

- ajouter, modifier, supprimer un véhicule
- consulter la liste des véhicules
- rechercher et filtrer (immatriculation, modèle, statut)
- accéder à la fiche détaillée d’un véhicule

### Informations d’un véhicule

- immatriculation  
- marque et modèle  
- année  
- kilométrage actuel  
- date du dernier entretien  
- statut (en service, hors service, en réparation)  
- type de carburant  

---

## Gestion des entretiens et réparations

Pour chaque véhicule :

- ajout d’une intervention (vidange, pneus, freins, contrôle technique)
- informations : garage, date, durée, coût
- mise à jour automatique du kilométrage et des dates
- consultation de l’historique complet

### Restrictions par rôle

- Mécanicien : ajout uniquement  
- Secrétaire : consultation, pas de modification des coûts  
- Patron : accès complet  

---

## Alertes de maintenance

Alertes automatiques basées sur :

- le kilométrage (ex : vidange tous les 15 000 km)
- la date (ex : contrôle technique tous les 2 ans)
- l’historique des entretiens
- des seuils paramétrables

Les alertes sont visibles :

- sur le tableau de bord
- sur la fiche du véhicule concerné

---

## Statistiques

Module réservé au rôle Patron :

- coût total par véhicule
- répartition des types d’interventions
- coûts mensuels et annuels
- top 5 des véhicules les plus coûteux
- nombre d’interventions par type
- graphiques d’évolution

---

## Exports

- export PDF du rapport complet d’un véhicule  
- export CSV de l’historique des entretiens  

---

## Technologies utilisées

- langage : C# (.NET)  
- interface : WPF  
- architecture : MVVM  
- base de données : SQLite  
- accès aux données : LINQ / Entity Framework  
- outils : Visual Studio  

---

## Sécurité

- mots de passe hashés  
- requêtes paramétrées  
- gestion stricte des droits par rôle  
- aucune donnée sensible stockée en clair  

---

## Ergonomie

- interface intuitive et claire  
- thème clair (option sombre possible)  
- écrans épurés  
- boutons explicites  
- dialogues de confirmation pour les actions critiques  

---

## Livrables

- cahier des charges  
- diagrammes UML (cas d’utilisation et classes)  
- schéma de base de données  
- maquettes WPF  
- code source  
- exécutable final  
- dossier technique et utilisateur  
- présentation orale  

---

## Critères de réussite

Le projet est validé si :

- toutes les fonctionnalités sont implémentées
- les rôles fonctionnent correctement
- la base de données est cohérente
- l’interface est stable et ergonomique
- les statistiques sont exploitables
- la présentation est professionnelle
