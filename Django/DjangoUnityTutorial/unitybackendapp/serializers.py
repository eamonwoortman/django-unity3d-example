from django.contrib.auth.models import User
from rest_framework import serializers
from unitybackendapp.models import Score, Savegame

class ScoreSerializer(serializers.ModelSerializer):
    class Meta:
        model = Score


class CreateUserSerializer(serializers.ModelSerializer):
    email = serializers.EmailField(required=True)

    class Meta:
        model = User
        fields = ('email', 'username', 'password')
        extra_kwargs = {'password': {'write_only': True}}

    def create(self, validated_data):
        user = User(
            email=validated_data['email'],
            username=validated_data['username']
        )
        user.set_password(validated_data['password'])
        user.save()
        return user

class SavegameDetailSerializer(serializers.ModelSerializer):
    class Meta:
        fields = ('name', 'saveblob')
        model = Savegame


class SavegameListSerializer(serializers.ModelSerializer):
    class Meta:
        fields = ('id', 'name', 'updated')
        model = Savegame