# ECR Repository
resource "aws_ecr_repository" "app_repo" {
  name         = "espectaculos-web"
  force_delete = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "espectaculos-web"
  }
}

# Application Load Balancer
resource "aws_lb" "eks_alb" {
  name               = "eks-external-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = [aws_subnet.public_a.id, aws_subnet.public_b.id]
  
  idle_timeout = 2000

  tags = {
    Name = "eks-external-alb"
  }
}

# Target Group
resource "aws_lb_target_group" "eks_tg" {
  name     = "eks-node-tg"
  port     = 30080
  protocol = "HTTP"
  vpc_id   = aws_vpc.main.id

  health_check {
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    healthy_threshold   = 2
    unhealthy_threshold = 2
    timeout             = 5
    interval            = 15
    matcher             = "200-399"
  }
}

# Listener
resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.eks_alb.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.eks_tg.arn
  }
}

# EKS Cluster
resource "aws_eks_cluster" "main" {
  name     = "eks-lab-cluster"
  role_arn = "arn:aws:iam::466060356317:role/c186660a4830571l12339800t1w466060-LabEksClusterRole-UAoaMXxwnHCl"

  vpc_config {
    subnet_ids = [aws_subnet.private_a.id, aws_subnet.private_b.id]
  }

  version = "1.29"
}

# EKS Node Group
resource "aws_eks_node_group" "default" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "eks-lab-nodes"
  node_role_arn   = "arn:aws:iam::466060356317:role/c186660a4830571l12339800t1w466060356-LabEksNodeRole-jHAS2pPr2WQi"

  subnet_ids = [aws_subnet.private_a.id, aws_subnet.private_b.id]

  scaling_config {
    desired_size = 2
    max_size     = 4
    min_size     = 2
  }

  instance_types = ["t3.medium"]

  ami_type = "AL2_x86_64"
  capacity_type = "ON_DEMAND"

  # ASIGNAR EL SG
  launch_template {
    id      = aws_launch_template.eks_nodes.id
    version = "$Latest"
  }
}

# Stable Attachment: ASG → Target Group
resource "aws_autoscaling_attachment" "eks_asg_attachment" {
  autoscaling_group_name = aws_eks_node_group.default.resources[0].autoscaling_groups[0].name
  lb_target_group_arn    = aws_lb_target_group.eks_tg.arn

  depends_on = [
    aws_eks_node_group.default,
    aws_lb_target_group.eks_tg
  ]
}

resource "aws_launch_template" "eks_nodes" {
  name = "eks-node-template"

  vpc_security_group_ids = [
    aws_security_group.nodes_sg.id,
  ]
}
