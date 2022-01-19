<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>

var settings = {
    "async": true,
    "crossDomain": true,
    "url": "https://api.kroger.com/v1/connect/oauth2/token",
    "method": "POST",
    "headers": {
      "Content-Type": "application/x-www-form-urlencoded",
      "Authorization": "Basic cmVtaW5kcHJvbW90ZWRwcm9kdWN0YXBwbGljYXRpb24tMTgyZTM5NGM5OWVmZjM4YTBjODI1NjRmMjQ1NTkxMzAyNTIxMDI4NDIyNDI0Mzg1NDg0OlNmSjBsSnJXQ2FiNUFXSG9NQ2dkVldGbW5ETFVPYkluZDJnN3JIRE8="
    },
    "data": {
      "grant_type": "client_credentials",
      "scope": "product.compact"
    }
  }

  $.ajax(settings).done(function (response) {
    console.log(response);
  });