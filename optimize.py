import subprocess
from bayes_opt import BayesianOptimization
from bayes_opt import UtilityFunction
from bayes_opt.logger import JSONLogger
from bayes_opt.event import Events

class ProcessLauncher(object):
    def __init__(self, process_path, args):
        self.process_path = process_path
        self.args = args

    def invoke(self):
        print(f"Invoking: {self.process_path} | {self.args}")
        DETACHED_PROCESS = 0x00000008
        cmd = self.process_path + " " + str(self.args // 1)
        results = subprocess.Popen(cmd, close_fds=True, creationflags=DETACHED_PROCESS, stdout=subprocess.PIPE)
        out = str(results.communicate()[0])
        return -float((out.replace('\\r', '').replace('\\n', '').replace("'", "").replace('b', '')))

def objective_function(loh) -> float:
    path =  r'C:\\Users\\musharm\\source\\repos\\NetOptimize\\GCDataExtractor\\bin\\Release\\net7.0\\GCDataExtractor.exe'
    p = ProcessLauncher(path, loh)
    return p.invoke() 

def fix_json(data):
   split = data.split("\n")
   fixed = "" 
   for line in split:
       if line == "":
           continue
       fixed += line + ","
   fixed = fixed[:-1]
   fixed = "[" + fixed + "]"
   return fixed
        

if __name__ == '__main__':

    pbounds = {'loh': (20_000, 2_000_000) } 
    optimizer = BayesianOptimization(
    f=objective_function,
    pbounds=pbounds,
    verbose=2, # verbose = 1 prints only when a maximum is observed, verbose = 0 is silent
    random_state=1,)

    logger = JSONLogger(path = './Result.json')
    optimizer.subscribe(Events.OPTIMIZATION_STEP, logger)

    # Prefer Exploitation = 0 and Prefer Exploration = 0.1
    EXPLORATION_EXPLOITATION_PARAMETER = 0.1 

    acquisition_function = UtilityFunction(kind="ei", xi=EXPLORATION_EXPLOITATION_PARAMETER)
    optimizer.maximize(init_points=0, n_iter= 30, acquisition_function = acquisition_function)

    print(optimizer.max)

    for i, res in enumerate(optimizer.res):
        print("Iteration {}: \n\t{}".format(i, res))

    with open('./Result.json') as f:
        data = fix_json(f.read())
        with open('./Result_fixed.json', 'w+') as wf:
            wf.write(data)