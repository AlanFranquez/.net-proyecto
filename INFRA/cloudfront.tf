#resource "aws_cloudfront_origin_access_identity" "frontend" {
#  comment = "OAI para acceder al bucket del frontend"
#}

#resource "aws_cloudfront_distribution" "frontend" {
#  enabled             = true
#  default_root_object = "index.html"
#
#  origin {
#    domain_name = aws_s3_bucket.frontend.bucket_regional_domain_name
#    origin_id   = "s3-frontend"
#
#    s3_origin_config {
#      origin_access_identity = aws_cloudfront_origin_access_identity.frontend.cloudfront_access_identity_path
#    }
#  }
#
#  default_cache_behavior {
#    allowed_methods  = ["GET", "HEAD"]
#    cached_methods   = ["GET", "HEAD"]
#    target_origin_id = "s3-frontend"
#
#    viewer_protocol_policy = "redirect-to-https"
#  }
#
#  restrictions {
#    geo_restriction {
#      restriction_type = "none"
#    }
#  }
#
#  viewer_certificate {
#    cloudfront_default_certificate = true
#  }
#}


#resource "aws_cloudfront_distribution" "backend" {
#  enabled = true
#
#  origin {
#    domain_name = aws_lb.app_alb.dns_name
#    origin_id   = "eks-backend"
#
#    custom_origin_config {
#      http_port              = 80
#      https_port             = 443
#      origin_protocol_policy = "http-only"
#      origin_ssl_protocols   = ["TLSv1.2"]
#    }
#  }

#  default_cache_behavior {
#    allowed_methods  = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
#    cached_methods   = ["GET", "HEAD", "OPTIONS"]
#    target_origin_id = "eks-backend"
#
#    viewer_protocol_policy = "redirect-to-https"
#  }

#  restrictions {
#    geo_restriction {
#      restriction_type = "none"
#    }
#  }

#  viewer_certificate {
#    cloudfront_default_certificate = true
#  }
#}
