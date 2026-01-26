Cahier des charges – Application de Gestion de Maintenance d’un Parc Automobile
1. Contexte du projet
Une entreprise possède un parc automobile composé de plusieurs véhicules utilisés au quotidien. Elle souhaite disposer d’un logiciel interne permettant de gérer l’ensemble des opérations de maintenance afin d’assurer le suivi des entretiens, d’éviter les retards, et de maîtriser les coûts.
 Le logiciel doit être simple d’utilisation, sécurisé, et adapté aux différents rôles du personnel : patron, mécanicien et secrétaire.
Ce projet est réalisé dans le cadre du BTS SIO / SN / autres, par un groupe de deux étudiants, en C# avec interface graphique sous WPF.
________________________________________
2. Objectifs du projet
L’objectif principal est de développer une application desktop permettant :

●	de gérer un parc de véhicules,

●	d’enregistrer les opérations d’entretien et les réparations,

●	de suivre l’historique de maintenance,

●	de générer des alertes automatiques pour les entretiens périodiques,

●	de produire des statistiques sur l’état du parc et les coûts,

●	de sécuriser l’accès via une authentification avec rôles.

Le logiciel doit offrir une interface ergonomique, intuitive, et adaptée au poste de travail utilisé.
________________________________________
3. Périmètre fonctionnel
3.1 Connexion & Gestion des rôles
L’application doit proposer :

●	une page de connexion,

●	une vérification des identifiants dans une base de données,

●	un système de rôles avec permissions :

Rôle	Droits
Patron	Tous les droits (gestion véhicules, entretiens, coûts, statistiques, gestion utilisateurs)
Mécanicien	Consultation véhicules + ajout d’entretiens
Secrétaire	Gestion administrative (ajout véhicules, consultation historique) mais restrictions sur les coûts et suppressions
La connexion doit s’appuyer sur un mot de passe hashé (SHA256).
________________________________________
3.2 Gestion du parc automobile
L’utilisateur doit pouvoir :

●	ajouter, modifier, supprimer un véhicule,

●	consulter la liste des véhicules,

●	rechercher/filtrer par immatriculation, modèle, statut

●	consulter la fiche détaillée d’un véhicule.

Chaque véhicule doit contenir les informations suivantes :
●	immatriculation,

●	marque, modèle,

●	année,

●	kilométrage actuel,

●	date du dernier entretien,

●	statut (en service, hors service, en réparation),

●	type de carburant.

________________________________________
3.3 Gestion des entretiens & réparations
Pour chaque véhicule :

●	ajouter une intervention (vidange, freins, pneus, CT, etc.),

●	renseigner le garage, la date, le coût, la durée,

●	associer un modèle de type d’entretien,

●	mise à jour des données du véhicule (kilométrage, dates),

●	consultation de l’historique complet.

Restrictions :
●	le mécanicien peut ajouter mais pas supprimer,

●	le secrétaire peut consulter mais pas modifier les coûts,

●	le patron a tous les accès.

________________________________________
3.4 Alertes de maintenance
Le logiciel doit générer automatiquement des alertes basées sur :

●	le kilométrage (ex : vidange tous les 15 000 km),

●	la date (ex : contrôle technique tous les 2 ans),

●	la date d’un entretien précédent,

●	des seuils paramétrables dans la base.

Les alertes doivent apparaître :
●	sur le tableau de bord,

●	et sur la fiche du véhicule concerné.

________________________________________
3.5 Statistiques
Le module de statistiques doit offrir :

●	coût total par véhicule,

●	répartition des types d’interventions,

●	coûts mensuels / annuels,

●	top 5 véhicules les plus coûteux,

●	nombre d’interventions par type,

●	graphiques d’évolution (coûts, interventions).

Accès :
●	réservé au rôle patron.

________________________________________
3.6 Exports
L’application doit permettre :

●	export PDF d’un rapport complet d’un véhicule,

●	export CSV de l’historique des entretiens.

________________________________________
4. Contraintes techniques
4.1 Technologies
   
●	Langage : C# (.NET)

●	Interface : WPF

●	Architecture : MVVM

●	Base de données : SQLite

●	Outils : LINQ, Entity Framework (ou requêtes SQL)

4.2 Sécurité
●	Mots de passe hashés

●	Requêtes protégées (paramétrées)

●	Limitation des droits selon rôle

●	Pas de stockage de données sensibles en clair

4.3 Performance
●	Application fluide, même avec plusieurs centaines de véhicules

●	Chargement des listes optimisé

●	Mise en cache simple des données si nécessaire

4.4 Maintenance & Evolutivité
Le code doit être :
●	commenté,

●	organisé selon MVVM,

●	facilement extensible (ex : ajout de conducteurs, planning, pièces détachées).

________________________________________
5. Contraintes ergonomiques
L’interface doit :

●	être intuitive,

●	proposer un thème clair (et éventuellement sombre),

●	afficher peu d’informations par écran,

●	utiliser des boutons clairement identifiés,

●	proposer des dialogues de confirmation pour les actions critiques.

________________________________________
6. Livrables
   
●	Cahier des charges (ce document)

●	Modèle UML (cas d’utilisation + classes)

●	Schéma de la base de données

●	Maquettes des interfaces WPF

●	Code source complet

●	Exécutable final

●	Dossier technique + dossier utilisateur

●	Présentation orale

________________________________________
7. Critères de réussite
Le projet sera considéré comme réussi si :

●	toutes les fonctionnalités décrites sont présentes,

●	les rôles fonctionnent correctement,

●	la base de données est cohérente et fonctionnelle,

●	l’interface est ergonomique et stable,

●	les statistiques sont correctes et exploitables,

●	le logiciel est présenté de manière professionnelle.


