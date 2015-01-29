![Unity_3D_logo.png](https://cloud.githubusercontent.com/assets/9072397/5611600/db8770ac-94c8-11e4-976a-9e42ccf23345.png)
![python-django.png](https://cloud.githubusercontent.com/assets/9072397/5611563/762c7108-94c8-11e4-9d9c-8ae4a703a03e.png)
# Django-Unity-Example #

Django-Unity-Example is an example project of how to use Django as a backend for a Unity3D game. 


### Overview ###

Creating backends for your games has never been this easy! Django and the Django Rest Framework are excellent tools to
As this is just an example project, this does *not* cover deploying or getting ready for production.


* Players can signup using an email adress, username and password
* Players can login
* Players will be able to save their score and get a list of all scores
* Players can save their game(blob of data) and retreive a list of saved games

### This project will use the following tools and packages ###

* Django v1.7.1
* Django-rest-framework v3.0.2
* Unity3D v4.3+
* MS Visual Studio Community 2013 
* Python tools v2.1 for Visual Studio 

### How do I get set up? ###
***Django***

We start of by installing our tools for working with/on our Django project.

* Download and install Visual Studio 2013
* Download and install Python 2.7
* Download and install Python Tools for Visual Studio

After we have installed the required tools, we have to ready our environment. We do this by creating a new virtual environment and synchronizing the database.

* Open the solution and either choose or set up a virtual environment(right-click Environments->Add virtual environment), name it 'django-unity'
* Synchronize the database by right-clicking the project->Python->Django Sync DB...
* You will be asked to enter a username and password for the super user, use 'admin' for both username and password
You are now done.


***Unity3D***

* Download and install Unity3D (v4.3 or higher)
* Simply open the Unity project in the Unity editor

*NOTE: depending on which Unity version you are using, you may get a decrepation warning as this project was created using Unity 4.3*

***Django on Linux/Mac***
The project is currently set up using Microsoft Visual Studio and Python Tools for VS. If you are using either Linux or Mac, you should be able to run this django project considered you've got the dependencies(django, restframework). 


### Getting started ###

***Django***

* If you haven't already done so, open the solution in Visual Studio 
* Start debugging by pressing F5 or clicking the Play button
*This will run the Django Debug server and open your webbrowser at http://localhost:8000/ which displays a friendly "Hello world" message*
* Optionally explore the admin page at http://localhost:8000/admin/ by logging in with the admin/admin credentials


***Unity3D***

* If you haven't already done so, open the solution in the Unity3D editor
* Either load the main scene which contains the tester or open one of the two example scenes
* Press play



### Documentation ###
The [wiki](https://github.com/eamonwoortman/django-unity3d-example/wiki) contains more information about the Django and Unity project.

NOTE: THIS EXAMPLE PROJECT IS NOT FINISHED YET

### Contributor  ###
Bas 'broding' Roding

### Licence ###

*Django-Unity-Example* is licenced under the MIT License. Please see the LICENSE file for details.
