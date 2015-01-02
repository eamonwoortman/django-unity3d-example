from django.contrib.auth.models import User
from django.utils.datastructures import MultiValueDict
from rest_framework import authentication, permissions
from rest_framework import status
from rest_framework import parsers
from rest_framework import renderers
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.generics import DestroyAPIView, GenericAPIView
from rest_framework.authtoken.models import Token
from rest_framework.authtoken.serializers import AuthTokenSerializer
from rest_framework import mixins
from unitybackendapp.serializers import ScoreSerializer, CreateUserSerializer
from unitybackendapp.models import Score

class UnityAPIView(GenericAPIView):
    """
    The UnityAPIView replaces the response status code with 200 
    and adds a REAL_STATUS_CODE header containing the original status code. 
    This is required to work with Unity3D's WWW class since 
    it doesn't read the response body if the status code is anything other than 200
    """
    def finalize_response(self, request, *args, **kwargs):
        response = super(UnityAPIView, self).finalize_response(request, *args, **kwargs)
        response["REAL_STATUS"] = '%s %s' % (response.status_code, response.status_text)
        response.status_code = 200
        return response

class ScoreAPI(mixins.ListModelMixin,
               UnityAPIView):
    authentication_classes = (authentication.TokenAuthentication,)
    permission_classes = (permissions.IsAuthenticatedOrReadOnly,)
    
    queryset = Score.objects.all()
    serializer_class = ScoreSerializer

    def get(self, request, *args, **kwargs):
        return self.list(request, *args, **kwargs)

    def get_object(self, pk, score):
        try:
            return Score.objects.get(user=pk, score=score)
        except Score.DoesNotExist:
            return None

    def post(self, request, format=None):
        """
        Post a new score
        """
        data = MultiValueDict(request.DATA)
        #force the authenticated user as the owner
        data['user'] = request.user.pk
        #first try to see if there already exists an object with that user and score
        score = self.get_object(data['user'], data['score'])
        if score != None:
            serializer = ScoreSerializer(score, data=data)
        else:
            serializer = ScoreSerializer(data=data)

        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class RegisterUser(UnityAPIView):
    def post(self, request, format=None):
        serializer = CreateUserSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    
class DeleteUser(UnityAPIView):
    def post(self, request, format=None):
        try:
            user = User.objects.get(username=request.data['username'], email=request.data['email'])
            if user.check_password(request.data['password']) is False:
                raise Exception('Invalid password')
            user.delete()
        except:
            return Response('Could not find that user', status=status.HTTP_400_BAD_REQUEST)
        return Response(status=status.HTTP_204_NO_CONTENT)

class GetAuthToken(UnityAPIView):
    throttle_classes = ()
    permission_classes = ()
    parser_classes = (parsers.FormParser, parsers.MultiPartParser, parsers.JSONParser,)
    renderer_classes = (renderers.JSONRenderer,)

    def post(self, request):
        serializer = AuthTokenSerializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        user = serializer.validated_data['user']
        token, created = Token.objects.get_or_create(user=user)
        return Response({'token': token.key})

