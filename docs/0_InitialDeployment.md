# Kubernetes and HELM deployment using Azure Container Services

This deployment use Azure Container Service (ACS) as a infrastruture service for Kubernetes Cluster, including HELM as a package management for K8s.

Also this setup install Kubernetes Operational View as a read-only system dashboard for K8s Cluster. More information [(here)](https://github.com/hjacobs/kube-ops-view)

## Requirements ##

- Azure account [(get started for free)](https://azure.microsoft.com/en-us/free/)
- Azure CLI 2.0 installed [(click here to install)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)


## Create separated Resource Group ##
Create a dedicated Resource Group to host your Azure Container Service and K8s cluster. This example create the Resource Group on West US region

```azurecli
az group create --name HaraRG11 --location westus
```

## Create the ACS using Kubernetes ##
This command create ACS using Kubernetes as a orchestrator with 2 agents and automatically generate SSH Keys

```azurecli
az acs create --orchestrator-type=kubernetes --resource-group HaraRG11 --name=harak8srg11 --dns-prefix=harak8srg11 --agent-count=2 --generate-ssh-keys
```
Wait few minutes until the process finish. After the deployment is finished then you can check if everything is working fine

```azurecli
az acs show -g HaraRG11 -n harak8srg11
```


### Extra commands ###

You can add the following options:
- Increase number os masters (default=1)
```
--master-count=2
```
This option will create 2 masters

- Choose size of the agents (default=Standard_D2_v2)
```
--agent-vm-size=Standard_D3_v2
```
This option will create agents using Standard D3_v2

------------------------------------------------------------------------------------


Install K8s command line (Kubectl)

```azurecli
sudo az acs kubernetes install-cli
```
Copy the credentials from Kubernetes (on ACS) to kubectl locally

```azurecli
az acs kubernetes get-credentials --resource-group=hararg11 --name=harak8srg11
```

Now you can check the installation of Kubernetes using the following command:

```bash
kubectl cluster-info
```

Also you can inspect the version of Kubectl installed
```bash
kubectl version
```

![Image of kubectl version](https://github.com/fabioharams/kubernetes/blob/master/img/kubectl_version.PNG)


To connect on your Kubernetes Dashboard just execute the following command bellow. This will enable a local proxy redirecting your local connection to Kubernetes on ACS

```bash
kubectl proxy &
```
Obs: remember to not close your shell

Open the following URL on your browser (eg: Chrome) to access the Kubernetes dashboard:
```
http://localhost:8001/ui
```

![Image of kubernetes dashboard](https://github.com/fabioharams/kubernetes/blob/master/img/dashboard.PNG)


### Extra - using namespaces ###
If you want to separate your environments (production, development, test, etc) then you can create Namespaces

Create a namespace for Dev environment:
```
kubectl create namespace dev
```

List namespaces
```bash
kubectl get namespaces
```

![Image of namespaces](img/namespaces.PNG)

Install NGINX on Dev Namespace with 3 replicas
```bash
kubectl run nginx --image=nginx --replicas=3 --namespace=dev
```

Verify all pods running on Dev Namespace
```bash
kubectl get pods --namespace=dev
```

Also you can inspect Pods and visualize using YAML format:
```bash
kubectl get pods nginx --namespace=dev -o yaml | grep -C 6 resources
```

If you want to delete namespace
```bash
kubectl delete namespace dev
```

### Extra 2 - create Pods using YAML ###
YAML is the standard format to create Pods using Kubectl command. The sample code bellow use busybox image. Save as busybox.yaml
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: busybox
  namespace: default
spec:
  containers:
  - image: busybox
    command:
      - sleep
      - "3600"
    imagePullPolicy: IfNotPresent
    name: busybox
  restartPolicy: Always
```

Create pods using YAML file
```bash
kubectl create -f busybox.yaml
```

After the process finished you can verify the endpoint exposed:
```bash
kubectl get ep
```




## Install HELM ##

The setup of HELM is easy and quite simple. Open shell and execute the following commands:
```bash
curl https://raw.githubusercontent.com/kubernetes/helm/master/scripts/get > get_helm.sh
chmod 700 get_helm.sh
./get_helm.sh
```

After the installation you can check if everything is going fine. 

### List Charts available ###
```bash
helm search
```

![Image of helm search](https://github.com/fabioharams/kubernetes/blob/master/img/helm_search.PNG)

### Install Chart to test ###
```bash
helm inspect stable/spartakus
helm install stable/spartakus
```

### Verify all Charts installed ###
```bash
helm ls
```

![Image of helm ls](https://github.com/fabioharams/kubernetes/blob/master/img/helm_ls.PNG)

### List the status of Charts ###
```bash
helm list
helm status name_of_chart_listed
```

## Monitor Kubernetes using Kube Ops View ##
You can install Kube-ops-view to monitor your Pods and Nodes using an browser. Follow these steps bellow to install and use Kube-Ops-View:
```bash
helm install --name=kubeopsview stable/kube-ops-view
```

After the installation you can open the browser and navigate to the URL:
```
http://localhost:8001/api/v1/proxy/namespaces/default/services/kubeopsview-kube-ops-view/
```

![Image of kubeopsview](https://github.com/fabioharams/kubernetes/blob/master/img/kubeops.PNG)

![Image of kubeopsview creating pod](https://github.com/fabioharams/kubernetes/blob/master/img/kubeops2.PNG)


## Extra - How to scale Agents ##

You can scale in/out the agents just using AZ command

### Increase/decrease the number of Agents ###

```
az acs scale -g HaraRG10 -n harak8srg11 --new-agent-count 3
```

This command will change the number of agents. 

