Grab Yer Assets v3
Copyright Frederic Bell 2014
----------------------------

Grab Yer Assets makes accessing and managing your Asset Store packages (*.unityPackage) a breeze.

It’s your Asset Store folder.  So “Grab Yer Assets” with both hands and.. well.. the rest is up to you!

-- Important:

When upgrading from v2 to v3, please remove the existing '../Plugins/Editor/GrabYerAssets' folder before installing v3.
If v3 is installed over v2, there is likely to be a conflict due to a left over file from v2, 'GrabYerAssets.cs'.
This file can safely be deleted as it has been replaced by 'GYA.cs'.

-- VERSION INFO:

3.16.x.x Major changes:

IMPORTANT: GYA v3 uses a different file setup from v2.  GYA's Settings file, 'GYA Settings.json', which contained both the Settings & Groups has been split up into seperate files, 'GYA Prefs.json' & 'GYA Groups.json'.
v3 will automatically import and backup your v2 (2.15c15j or newer) Settings and Groups the first time it is run.  
Changes made to the Settings or Groups using v2 AFTER the initial conversion, will NOT be carried forward to v3.  So, it is best to upgrade any projects using GYA at the same time, to avoid making changes with an older version.

-- Highlights:

Optimizations:
- Collection (Package/Folder) scanning re-write to add Indexing greatly improves subsequent 'Refresh' times
- UI/Scroll View re-write, can now display 10k+ assets without slowing down.
- Overall, a good size re-write with greatly improved code separation & numerous other internal optimizations.

These changes give GYA a noticeable performance leap which was the goal.
GYA v3 can now easily maintain/scan/display 10k+ packages.
GYA v2 would start to noticeably slowdown around 2k - 2.5k packages, probably less but it's been awhile since I tested and it gets progressively worse as the number of assets grow, for both the scanning times and the UI. GYA v3 doesn't have this problem.

UI improvements:
- Added a true 'Preferences' window for easy access to: Quick Reference, Preferences, User Folders, Groups, Maintenance, etc.
- Improved 'User Folder' & 'Group' management
- Version locking (optional) of packages within Groups. (AS packages only, n/a for Exported packages)
Right-click on any asset in the Group, select 'Lock Version for this Group' to select which version to lock.
Say that you have a project that REQUIRES a certain version of an asset, THAT version can be locked to it.
That way, if you want to use a Group more as a Project Group, you have the option.
- Improved multi-version list view and handling
- 3rd Toolbar (optional) added for Quick Access

Web data integration:
- Downloading and processing of the Users 'Purchased Assets' list
- Which allows detection of 'Deprecated' and 'Not Downloaded' assets from the Asset Store, sorting by Purchase Date, etc.

Features:
---------

- Colored collections for easy visual reference
- Groups, numerous groups can be created to further organize your assets
- Favorites is the default group
- Persist mode option to allow GYA to maintain a copy of itself in the Standard Assets folder for ease of access when starting new projects
- Easily access your assets (*.unityPackages): Asset Store & Standard Assets
- Supports a User Asset folder that can be on another drive or share
- Single & Multiple asset import (*), including group import
- Search by: Title, Category, Publisher
- Sort by: Title, Category, Subcategory, Publisher, Size, VersionID
- Info panel to show asset details 
- Automatically finds and shows any Old Versions
- Indicator notes which version is currently being looked at
- Show all available versions when selecting an Asset
- Consolidate (Move & Rename with version appended) Old Assets into a separate folder “/Asset Store/-Grab Yer Assets (Old)”.  Makes it easy to backup, delete or just to keep them separated from your updated assets.
- Direct access to Asset Folders
- Direct access to Asset URLs

Usage:
------
Searching and Sorting -
Click on the Search bar icon, magnifying glass, for Sort & Search options

User Asset Folder -
"Menu->User Asset Folder" to select an alternate asset folder to scan, sub-folders are handled

Persist -
Select this option to have GYA automatically keep an updated version of itself in the Standard Assets folder.  It will be updated anytime that a new version is been downloaded from the Asset Store and the "Refresh" button has been clicked.

Refresh -
Will manually rescan and update your asset/package info.
GYA does not automatically refresh your asset list, you have to do it manually.
Simply refresh after you have updated any packages from the Asset Store.

General Controls -
Left-Click: Toggle asset for multi-import (*)
Right-Click: Display popup to load the asset or open the assets folder or URL
Refresh Icon: Rescans the folders and rebuilds the asset/package list
Prev/Next Arrows: Cycles through the various Asset views (Asset Store, Standard Assets, Old Assets, User Assets and any Groups you may have created)

Notes (*):
- Unity 4.2 or newer is required for Multi-Import to function properly.

