import {ensureEnvironmentVariables} from './environment'

const validators = {
  environment: {
    checkEnvironment: () => {
      console.log('Checking environment')
      ensureEnvironmentVariables()
      console.log('Environment OK!')
    }
  }
}

export default validators