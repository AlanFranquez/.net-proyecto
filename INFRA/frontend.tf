resource "aws_s3_bucket" "frontend" {
  bucket = "mi-frontend-react-58b37209"
}

resource "aws_s3_bucket_ownership_controls" "frontend" {
  bucket = aws_s3_bucket.frontend.id
  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_public_access_block" "frontend" {
  bucket                  = aws_s3_bucket.frontend.id
  block_public_acls       = false
  ignore_public_acls      = false
  block_public_policy     = false
  restrict_public_buckets = false
}

# Política para permitir acceso público a los objetos
resource "aws_s3_bucket_policy" "frontend_policy" {
  bucket = aws_s3_bucket.frontend.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadGetObject"
        Effect    = "Allow"
        Principal = "*"
        Action    = "s3:GetObject"
        Resource  = "${aws_s3_bucket.frontend.arn}/*"
      }
    ]
  })
}

# Activar hosting estático
resource "aws_s3_bucket_website_configuration" "frontend_website" {
  bucket = aws_s3_bucket.frontend.id

  index_document {
    suffix = "index.html"
  }

  error_document {
    key = "index.html"
  }
}

# Subir tu build (opcional)
#resource "aws_s3_object" "frontend_files" {
#  for_each = fileset("${path.module}/build", "**/*")

#  bucket       = aws_s3_bucket.frontend.id
#  key          = each.value 
#  source       = "${path.module}/build/${each.value}"
#  etag         = filemd5("${path.module}/build/${each.value}")
#  content_type = lookup({
#    html = "text/html",
#    js   = "application/javascript",
#    css  = "text/css",
#    png  = "image/png",
#    jpg  = "image/jpeg",
#    svg  = "image/svg+xml"
#  }, split(".", each.value)[length(split(".", each.value)) - 1], "binary/octet-stream")
#}
