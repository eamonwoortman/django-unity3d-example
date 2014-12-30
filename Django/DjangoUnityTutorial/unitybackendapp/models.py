from django.db import models
from django.contrib import admin

# Create your models here.
class Score(models.Model):
    name = models.CharField(max_length=20)
    score = models.IntegerField()
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)

    def __unicode__(self):
        return '%s - %d' % (self.name, self.score)

admin.site.register(Score)